/// Date and number formatting for the Arabic UI.
///
/// Hand-rolled rather than pulled from `intl`, for two reasons. The design uses
/// the Levantine month names (آب، تموز، حزيران) that Jordan actually reads, and
/// it sets every figure in Western digits — `intl`'s `ar` locale would give
/// Arabic-Indic ones (١٢٤٠) and the wrong month set for this region. Adding the
/// package and then overriding both of its decisions would be more code than
/// this, not less.
library;

/// The Levantine month names, as Jordan writes them.
const List<String> _monthNames = [
  'كانون الثاني',
  'شباط',
  'آذار',
  'نيسان',
  'أيار',
  'حزيران',
  'تموز',
  'آب',
  'أيلول',
  'تشرين الأول',
  'تشرين الثاني',
  'كانون الأول',
];

/// `20 آب 2026` — the date alone, for an order card.
///
/// Takes a UTC timestamp and renders it in the device's local zone: the server
/// speaks UTC, the customer reads wall-clock time.
String formatArabicDate(DateTime utc) {
  final local = utc.toLocal();
  return '${local.day} ${_monthNames[local.month - 1]} ${local.year}';
}

/// `20 آب 2026 · 4:12 م` — the date with the time, for a ledger row.
///
/// Points move several times a day, so the ledger needs the clock to tell two
/// visits apart; an order card does not, and omits it.
String formatLedgerTimestamp(DateTime utc) {
  final local = utc.toLocal();
  return '${formatArabicDate(utc)} · ${_formatTime(local)}';
}

/// 12-hour time with the Arabic meridiem: `4:12 م`, `11:05 ص`.
String _formatTime(DateTime local) {
  // 0 and 12 both display as 12 — midnight is 12 ص, noon is 12 م.
  final hour12 = local.hour % 12 == 0 ? 12 : local.hour % 12;
  final minute = local.minute.toString().padLeft(2, '0');
  final meridiem = local.hour < 12 ? 'ص' : 'م';
  return '$hour12:$minute $meridiem';
}

/// Groups thousands the way the design shows them: `1240` → `1,240`.
///
/// Western digits and an ASCII comma, matching the balance in the design. The
/// input is a count of points and so never negative, which is why there is no
/// sign handling here — the ledger signs its own amounts.
String formatGroupedNumber(int value) {
  final digits = value.abs().toString();
  final buffer = StringBuffer();

  for (var i = 0; i < digits.length; i++) {
    if (i > 0 && (digits.length - i) % 3 == 0) buffer.write(',');
    buffer.write(digits[i]);
  }

  return buffer.toString();
}

/// A money amount in dinars, always to two decimals: `3.5` → `3.50`.
///
/// The wire sends a JSON number for a server-side decimal, so `3.5` and `3.50`
/// arrive identically; the menu and the order totals must not show one price
/// with one decimal place beside another with two.
String formatMoney(double value) => value.toStringAsFixed(2);

/// A quantity, with the trailing zeros a whole number does not need.
///
/// Beans sell by weight, so `0.5` must survive as `0.5`, while a coffee bought
/// twice reads `2` rather than `2.00`.
String formatQuantity(double value) {
  if (value == value.roundToDouble()) return value.toInt().toString();
  return value
      .toStringAsFixed(3)
      .replaceFirst(RegExp(r'0+$'), '')
      .replaceFirst(RegExp(r'\.$'), '');
}
