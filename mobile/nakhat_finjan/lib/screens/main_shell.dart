import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../providers/customer_provider.dart';
import '../widgets/app_bottom_nav.dart';
import 'home_screen.dart';
import 'menu_screen.dart';
import 'purchases_screen.dart';
import 'settings_screen.dart';

/// Everything behind the login: four tabs under one bottom bar.
///
/// The bodies live in an [IndexedStack] rather than being rebuilt on each tap,
/// so the menu keeps its selected category and each list keeps its scroll
/// position when the customer comes back to it.
class MainShell extends StatefulWidget {
  const MainShell({super.key, this.initialTab = AppTab.home});

  final AppTab initialTab;

  @override
  State<MainShell> createState() => _MainShellState();
}

class _MainShellState extends State<MainShell> {
  late AppTab _current = widget.initialTab;

  @override
  void initState() {
    super.initState();

    // Fetch once, here, rather than in each tab's own initState: all four
    // bodies mount together inside the IndexedStack, so per-tab loads would
    // fire simultaneously anyway — and any tab the customer never opens would
    // still have paid for its call.
    //
    // Deferred by a frame because a load can complete synchronously from cache
    // and notify listeners mid-build, which Flutter will not have.
    //
    // Not awaited: the shell paints its skeletons immediately and each section
    // fills in as it arrives.
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!mounted) return;
      context.read<CustomerProvider>().loadAll();
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: IndexedStack(
        index: _current.index,
        children: const [
          HomeScreen(),
          MenuScreen(),
          PurchasesScreen(),
          SettingsScreen(),
        ],
      ),
      bottomNavigationBar: AppBottomNav(
        current: _current,
        onSelected: (tab) => setState(() => _current = tab),
      ),
    );
  }
}
