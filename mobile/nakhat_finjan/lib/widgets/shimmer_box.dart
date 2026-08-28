import 'package:flutter/material.dart';

import '../theme/app_colors.dart';

/// One sweeping placeholder bar. The home screen's loading frame is built
/// entirely from these, sized to the copy they stand in for so the skeleton has
/// the same rhythm as the real list and nothing jumps when it resolves.
class ShimmerBox extends StatefulWidget {
  const ShimmerBox({
    super.key,
    required this.width,
    required this.height,
    this.radius = 6,
  });

  final double width;
  final double height;
  final double radius;

  @override
  State<ShimmerBox> createState() => _ShimmerBoxState();
}

class _ShimmerBoxState extends State<ShimmerBox>
    with SingleTickerProviderStateMixin {
  late final AnimationController _controller = AnimationController(
    vsync: this,
    duration: const Duration(milliseconds: 1400),
  )..repeat();

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: _controller,
      builder: (context, _) {
        // The gradient is four viewport-widths wide and slides one full sweep
        // per cycle, which is what gives the highlight its long, slow travel.
        final t = _controller.value;
        return ShaderMask(
          blendMode: BlendMode.srcIn,
          shaderCallback: (bounds) => LinearGradient(
            begin: Alignment(-1 - 4 * (1 - t), 0),
            end: Alignment(1 + 4 * (1 - t), 0),
            colors: const [
              Color(0x0F191817),
              Color(0x1F191817),
              Color(0x0F191817),
            ],
            stops: const [0.25, 0.37, 0.63],
          ).createShader(bounds),
          child: Container(
            width: widget.width,
            height: widget.height,
            decoration: BoxDecoration(
              color: AppColors.ink,
              borderRadius: BorderRadius.circular(widget.radius),
            ),
          ),
        );
      },
    );
  }
}
