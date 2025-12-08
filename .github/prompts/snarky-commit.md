---
trigger: commit
---

# Snarky Commit & PR

Build, test, check coverage, commit with attitude, and open a PR with maximum sass.

## What this does

This prompt will execute a comprehensive quality check and commit workflow:

1. **Build** the application in Release configuration
2. **Run all unit tests** to ensure nothing is broken
3. **Check code coverage** to ensure it hasn't dropped significantly (70% threshold)
4. **Smoke test** the application to verify it actually runs
5. **Create a feature branch** if you're currently on main/master
6. **Commit changes** with a randomly selected snarky commit message
7. **Push to remote** and set upstream tracking
8. **Open a pull request** with a spicy, attitude-filled title and description

## Prerequisites

- .NET SDK installed
- Python 3 installed (for coverage checking)
- Git repository initialized
- GitHub CLI (`gh`) installed and authenticated for auto-PR creation
  - Install: `brew install gh` (macOS) or `sudo apt install gh` (Linux)
  - Authenticate: `gh auth login`

## Quality Gates

The script will exit if any of these fail:

- ❌ Build fails
- ❌ Any test fails
- ❌ Code coverage drops below 70%
- ❌ Application fails to start

## Safety Features

- Never commits directly to main or master branch
- Always creates a feature branch with timestamp
- Validates all changes before committing
- Pushes with upstream tracking configured

## Sample Output

You'll get randomly selected snarky messages like:

**Commit Messages:**

- "Fixed the thing. You know, THAT thing. 🙄"
- "This commit is chef's kiss 👌 Your code review? Probably not."
- "Made the build green. Made the reviewers green with envy 💚"

**PR Titles:**

- "🔥 This PR is hotter than your last performance review"
- "💪 Flex on 'em: The Commit"
- "✨ Fixed everything. You're welcome."

## Usage

Just select this prompt in Agent Chat and let the automation work its magic!

---

Run the snarky commit script:

```bash
cd /workspaces/Demo1 && ./scripts/commit.sh
```
