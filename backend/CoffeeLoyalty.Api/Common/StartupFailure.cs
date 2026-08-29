namespace CoffeeLoyalty.Api.Common;

/// <summary>
/// Writes the reason a startup check failed somewhere a person can actually reach.
/// </summary>
/// <remarks>
/// This exists because of how IIS in-process hosting fails. A configuration check that throws
/// before the host is listening kills the worker process, and all ANCM records is
/// <c>"process terminated unexpectedly, exit code 0xffffffff"</c> in the Windows event log —
/// a log that, on shared hosting, the person who deployed the app cannot open. Turning on
/// <c>stdoutLogEnabled</c> is the usual advice and routinely produces an empty file, because
/// the process died before that redirection flushed anything.
/// <para>
/// So the message is written to <c>logs/startup-error.log</c> beside the application, which is
/// reachable over FTP or a hosting file manager. It is appended to rather than overwritten:
/// two different failures on two deploys are a far more useful pair than the second one alone.
/// </para>
/// </remarks>
public static class StartupFailure
{
    /// <summary>Folder the file is written into, relative to the content root.</summary>
    private const string LogDirectoryName = "logs";

    /// <summary>Name of the file itself.</summary>
    private const string LogFileName = "startup-error.log";

    /// <summary>
    /// Logs <paramref name="exception"/> through the normal logger and appends it to
    /// <c>logs/startup-error.log</c>.
    /// </summary>
    /// <param name="contentRootPath">The application's content root.</param>
    /// <param name="logger">The application logger, tried first.</param>
    /// <param name="exception">The failure to record.</param>
    public static void Record(string contentRootPath, ILogger logger, Exception exception)
    {
        logger.LogCritical(exception, "Startup failed: {Message}", exception.Message);

        try
        {
            var directory = Path.Combine(contentRootPath, LogDirectoryName);
            Directory.CreateDirectory(directory);

            var entry =
                $"""

                ===== startup failed {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss} UTC =====
                content root : {contentRootPath}
                exception    : {exception.GetType().FullName}
                message      : {exception.Message}

                {exception}

                """;

            File.AppendAllText(Path.Combine(directory, LogFileName), entry);
        }
        catch (Exception writeFailure)
        {
            // A read-only or full disk must not replace the real error with this one. The
            // original is already on the logger and is about to be rethrown either way.
            logger.LogError(
                writeFailure,
                "Could not write the startup failure to {Directory}.",
                Path.Combine(contentRootPath, LogDirectoryName));
        }
    }
}
