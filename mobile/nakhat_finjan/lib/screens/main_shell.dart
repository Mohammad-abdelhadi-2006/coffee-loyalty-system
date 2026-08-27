import 'package:flutter/material.dart';

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
