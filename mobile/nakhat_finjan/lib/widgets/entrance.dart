import 'dart:math' as math;

import 'package:flutter/material.dart';

/// Staggered entrance motion, shared by every screen.
///
/// The shape of it: one [EntranceGroup] per screen owns a single
/// [AnimationController], and each [EntranceItem] under it reads a slice of that
/// controller chosen by its index. One controller per screen rather than one per
/// item is the whole performance story — thirty rows animating off thirty
/// tickers is thirty times the bookkeeping for the same picture.
///
/// Three rules hold everywhere:
///
///  * **Once.** A group plays on its first appearance and never again. Data
///    refreshing, a `setState`, or coming back to a cached tab all leave it
///    alone.
///  * **Never bouncy.** Every curve here is an ease-out. Things rise and settle;
///    nothing overshoots and comes back.
///  * **Reduce motion wins.** When the platform asks for fewer animations, the
///    group never starts a controller and the items render as their bare
///    children — not a completed animation, no wrapper at all.

/// Whether the subtree is the tab currently on screen.
///
/// The four tabs live in an `IndexedStack`, so all of them are built the moment
/// the shell mounts. Without this an entrance would play against a tab nobody is
/// looking at, and be over before they switched to it. The shell publishes which
/// tab is showing; a group waits for its own to be the one.
///
/// Defaults to `true`, so a screen used outside the shell — a pushed route, a
/// widget test — animates on mount as you would expect.
class TabVisibility extends InheritedWidget {
  const TabVisibility({
    super.key,
    required this.isActive,
    required super.child,
  });

  final bool isActive;

  static bool of(BuildContext context) {
    final scope = context.dependOnInheritedWidgetOfExactType<TabVisibility>();

    return scope?.isActive ?? true;
  }

  @override
  bool updateShouldNotify(TabVisibility oldWidget) =>
      isActive != oldWidget.isActive;
}

/// The controller and timings, handed down to the items below.
class _EntranceScope extends InheritedWidget {
  const _EntranceScope({
    required this.animation,
    required this.stagger,
    required this.itemDuration,
    required this.total,
    required this.maxSteps,
    required this.disabled,
    required super.child,
  });

  final Animation<double> animation;
  final Duration stagger;
  final Duration itemDuration;
  final Duration total;
  final int maxSteps;

  /// Reduce motion. Items check this first and opt out entirely.
  final bool disabled;

  static _EntranceScope? maybeOf(BuildContext context) =>
      context.dependOnInheritedWidgetOfExactType<_EntranceScope>();

  /// The slice of the run belonging to [index], as a curve over the whole.
  Animation<double> curveFor(int index, {Curve curve = Curves.easeOut}) =>
      _slice(_offsetMsFor(index), curve);

  /// The slice for one word of a heading: its item's offset, plus the word's own
  /// smaller step inside it.
  Animation<double> wordCurve(int index, int wordIndex, Duration wordStagger) =>
      _slice(
        _offsetMsFor(index) + wordStagger.inMilliseconds * wordIndex,
        Curves.easeOut,
      );

  /// Milliseconds from the start of the run to when [index] begins.
  int _offsetMsFor(int index) =>
      stagger.inMilliseconds * math.min(index, maxSteps);

  /// One item's window, as a curve over the controller's 0..1.
  Animation<double> _slice(int offsetMs, Curve curve) {
    final totalMs = total.inMilliseconds;
    final start = math.min(0.999, offsetMs / totalMs);
    final end = math.min(1.0, start + itemDuration.inMilliseconds / totalMs);

    return CurvedAnimation(
      parent: animation,
      curve: Interval(start, end, curve: curve),
    );
  }

  @override
  bool updateShouldNotify(_EntranceScope oldWidget) =>
      animation != oldWidget.animation || disabled != oldWidget.disabled;
}

/// Owns one screen's entrance.
///
/// Put this above the elements that should stagger in, then give each of them an
/// [EntranceItem] with its position in the run.
class EntranceGroup extends StatefulWidget {
  const EntranceGroup({
    super.key,
    required this.child,
    this.ready = true,
    this.itemDuration = const Duration(milliseconds: 280),
    this.stagger = const Duration(milliseconds: 65),
    this.maxSteps = 8,
  });

  final Widget child;

  /// Whether the content this group wraps is the real thing.
  ///
  /// Screens that load pass `false` while the skeleton or spinner is up, so the
  /// entrance belongs to the data rather than playing against a placeholder and
  /// then having to play again when the rows arrive.
  final bool ready;

  /// How long one item takes to fade and rise.
  final Duration itemDuration;

  /// The gap between one item starting and the next.
  final Duration stagger;

  /// After this many items every later one shares the last delay, so a long
  /// list does not leave its thirtieth row waiting two seconds.
  final int maxSteps;

  @override
  State<EntranceGroup> createState() => _EntranceGroupState();
}

class _EntranceGroupState extends State<EntranceGroup>
    with SingleTickerProviderStateMixin {
  late final Duration _total =
      widget.itemDuration + widget.stagger * widget.maxSteps;

  late final AnimationController _controller = AnimationController(
    vsync: this,
    duration: _total,
  );

  /// Whether the run is currently "on" — the tab is showing and its data is
  /// here. Kept so a rebuild can tell an ordinary `setState` (no replay) from
  /// the screen actually being arrived at again (replay).
  bool _showing = false;

  bool _reduceMotion = false;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();

    // MediaQuery is inherited, so this is the earliest it can be read.
    _reduceMotion = MediaQuery.disableAnimationsOf(context);

    _sync();
  }

  @override
  void didUpdateWidget(EntranceGroup oldWidget) {
    super.didUpdateWidget(oldWidget);

    // Covers the group being mounted while its data was still loading and
    // `ready` having just turned true.
    _sync();
  }

  /// Starts the run when the screen is arrived at, and rewinds it when it is
  /// left so the next arrival plays again.
  ///
  /// The distinction that matters: this keys off *appearing*, not off building.
  /// A `setState`, a pull-to-refresh, or new data landing all rebuild the screen
  /// while it is already showing, and none of them replay anything — re-running
  /// the entrance under someone who is reading the page would be its own bug.
  void _sync() {
    final showing = widget.ready && TabVisibility.of(context);

    if (showing == _showing) return;

    _showing = showing;

    if (!showing) {
      // Left the tab. Rewind while nobody is looking, so coming back is a
      // fresh arrival rather than an already-finished screen.
      _controller.value = 0;
      return;
    }

    if (_reduceMotion) {
      // Straight to the end. No ticker is ever started, and the items below
      // render bare anyway — this only keeps the animation's value honest for
      // anything that reads it.
      _controller.value = 1;
      return;
    }

    _controller.forward(from: 0);
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return _EntranceScope(
      animation: _controller,
      stagger: widget.stagger,
      itemDuration: widget.itemDuration,
      total: _total,
      maxSteps: widget.maxSteps,
      disabled: _reduceMotion,
      child: widget.child,
    );
  }
}

/// One element of a staggered entrance: fades up into place.
///
/// [index] is its position in the run, counted from the top of the screen. Two
/// items may share an index when they should arrive together.
class EntranceItem extends StatelessWidget {
  const EntranceItem({
    super.key,
    required this.index,
    required this.child,
    this.rise = 0.05,
    this.curve = Curves.easeOut,
  });

  final int index;
  final Widget child;

  /// How the item travels. Defaults to a plain ease-out — it decelerates into
  /// place and stops. Pass something with a little overshoot where a hop is
  /// wanted, as the nav icons do.
  final Curve curve;

  /// How far the item starts below its resting place, as a fraction of its own
  /// height. Small on purpose — this is a settle, not an arrival.
  final double rise;

  @override
  Widget build(BuildContext context) {
    final scope = _EntranceScope.maybeOf(context);

    // No group above, or reduce motion: the child, exactly as it would have
    // been. Nothing wraps it, so there is nothing to animate or repaint.
    if (scope == null || scope.disabled) return child;

    final animation = scope.curveFor(index, curve: curve);

    // FadeTransition and SlideTransition both drive their render object from
    // the animation, so `child` is built once and only repainted — the subtree
    // below never rebuilds per frame.
    return RepaintBoundary(
      child: FadeTransition(
        // The fade always eases out, whatever the travel does: an overshooting
        // opacity would flicker past 1 and back.
        opacity: scope.curveFor(index),
        child: SlideTransition(
          position: Tween<Offset>(
            begin: Offset(0, rise),
            end: Offset.zero,
          ).animate(animation),
          child: child,
        ),
      ),
    );
  }
}

/// A short line of text that arrives a word at a time.
///
/// **Short strings only** — a title, a heading, a group label. Arabic paragraphs
/// revealed word by word are slow to read and quickly irritating; body copy gets
/// a plain [EntranceItem] around the whole block instead.
///
/// Words start from the right, which in Arabic is the start of the line, so the
/// reveal runs the way the sentence is read. The default gap is deliberately
/// tighter than the item stagger: a three-word heading is fully up in about a
/// tenth of a second longer than a one-word one.
class WordReveal extends StatefulWidget {
  const WordReveal(
    this.text, {
    super.key,
    this.style,
    this.startIndex = 0,
    this.wordStagger = const Duration(milliseconds: 40),
    this.textDirection = TextDirection.rtl,
  });

  final String text;
  final TextStyle? style;

  /// Where this heading sits in the surrounding run, so it arrives in step with
  /// the items around it.
  final int startIndex;

  final Duration wordStagger;
  final TextDirection textDirection;

  @override
  State<WordReveal> createState() => _WordRevealState();
}

class _WordRevealState extends State<WordReveal> {
  Animation<double>? _watched;

  /// Once the run is over this goes back to being one ordinary [Text].
  ///
  /// The split is a transient presentation detail and nothing more. Leaving the
  /// words as separate widgets would leave the heading permanently fragmented —
  /// a screen reader would announce the pieces one at a time, line breaking
  /// would be the Wrap's rather than the text's, and any lookup of the whole
  /// string would miss it. So at rest this widget is exactly what it was before
  /// the animation existed.
  bool _finished = false;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();

    final scope = _EntranceScope.maybeOf(context);

    if (scope == null || scope.disabled) {
      _detach();
      _finished = true;
      return;
    }

    _attach(scope.animation);
  }

  void _attach(Animation<double> animation) {
    if (identical(_watched, animation)) return;

    _detach();
    _watched = animation;

    _finished =
        animation.status == AnimationStatus.completed || animation.value >= 1;

    // Listening either way: the same controller is rewound and run again on the
    // next arrival, so this widget has to hear both ends of it.
    animation.addStatusListener(_onStatus);
  }

  void _detach() {
    _watched?.removeStatusListener(_onStatus);
    _watched = null;
  }

  void _onStatus(AnimationStatus status) {
    if (!mounted) return;

    // Tracks the run in both directions now that a group replays on every
    // arrival: collapse to one Text when it lands, split back into words when
    // it starts over. Without the second half the heading would animate once
    // and then never again, however many times the screen was opened.
    final finished = status == AnimationStatus.completed;

    if (finished == _finished) return;

    setState(() => _finished = finished);
  }

  @override
  void dispose() {
    _detach();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final scope = _EntranceScope.maybeOf(context);

    if (scope == null || scope.disabled || _finished) {
      return Text(
        widget.text,
        style: widget.style,
        textDirection: widget.textDirection,
      );
    }

    final text = widget.text;
    final style = widget.style;
    final textDirection = widget.textDirection;
    final startIndex = widget.startIndex;
    final wordStagger = widget.wordStagger;

    final words = text.split(' ').where((word) => word.isNotEmpty).toList();

    // One word is not a stagger. Skipping the Wrap also keeps the common case —
    // «المنيو», «الإعدادات», «مشترياتي» — a single Text, laid out and shaped
    // exactly as it was before.
    if (words.length < 2) {
      return EntranceItem(
        index: startIndex,
        child: Text(text, style: style, textDirection: textDirection),
      );
    }

    // The whole string is announced as one label while the pieces are on
    // screen, so assistive technology never hears a heading arrive in
    // fragments.
    return Semantics(
      label: text,
      child: ExcludeSemantics(
        child: RepaintBoundary(
          child: Wrap(
            textDirection: textDirection,
            crossAxisAlignment: WrapCrossAlignment.center,
            children: [
              for (var i = 0; i < words.length; i++)
                _RevealedWord(
                  // The space rides along with its word so the line spaces exactly
                  // as the undivided string would; Wrap adds none of its own.
                  text: i == words.length - 1 ? words[i] : '${words[i]} ',
                  style: style,
                  textDirection: textDirection,
                  animation: scope.wordCurve(startIndex, i, wordStagger),
                ),
            ],
          ),
        ),
      ),
    );
  }
}

class _RevealedWord extends StatelessWidget {
  const _RevealedWord({
    required this.text,
    required this.style,
    required this.textDirection,
    required this.animation,
  });

  final String text;
  final TextStyle? style;
  final TextDirection textDirection;
  final Animation<double> animation;

  @override
  Widget build(BuildContext context) {
    return FadeTransition(
      opacity: animation,
      child: SlideTransition(
        position: Tween<Offset>(
          begin: const Offset(0, 0.22),
          end: Offset.zero,
        ).animate(animation),
        child: Text(text, style: style, textDirection: textDirection),
      ),
    );
  }
}
