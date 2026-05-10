#!/usr/bin/env python3
"""Fix deps.json paths after DLLs are moved to Lib/ directory."""
import json
import os
import sys


def fix_deps_json(deps_path):
    with open(deps_path) as f:
        deps = json.load(f)

    targets = deps.get("targets", {})

    for target_name, packages in targets.items():
        for pkg_name, pkg_data in packages.items():
            runtime = pkg_data.get("runtime")
            if not runtime:
                continue

            new_runtime = {}
            for old_path, meta in runtime.items():
                filename = os.path.basename(old_path)
                if pkg_name.startswith("MultiWall/"):
                    new_runtime[filename] = meta
                else:
                    new_runtime[f"Lib/{filename}"] = meta

            pkg_data["runtime"] = new_runtime

    with open(deps_path, "w") as f:
        json.dump(deps, f, indent=2)

    print(f"Fixed deps.json: {deps_path}")


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: fix_deps.py <deps.json path>")
        sys.exit(1)

    deps_path = sys.argv[1]
    if not os.path.exists(deps_path):
        print(f"Error: {deps_path} not found")
        sys.exit(1)

    fix_deps_json(deps_path)
