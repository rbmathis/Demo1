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
6. **Commit changes** with your custom snarky commit message (or a randomly selected one)
7. **Push to remote** and set upstream tracking
8. **Open a pull request** with your custom spicy title and description (or a randomly generated one)

## Usage Options

### Option 1: Fully Custom (Recommended)
Provide both commit message and PR title:
```bash
./scripts/commit.sh "Your snarky commit message 🔥" "Your spicy PR title ✨"
```

### Option 2: Custom Commit, Random PR Title
Provide just the commit message:
```bash
./scripts/commit.sh "Fixed all the things like a boss 💪"
```

### Option 3: Full Random (Original Behavior)
Let the script pick everything randomly:
```bash
./scripts/commit.sh
```

### Using with Agent Chat
When using this prompt in Agent Chat, you can specify:
- Custom commit message
- Custom PR title
- Or let the agent pick random snarky ones for you

Just tell the agent what you want, like:
- "Run snarky commit with message 'Crushed this feature 🎯'"
- "Run snarky commit with custom PR title 'Best code you'll see today 💎'"
- "Run snarky commit with random messages"

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

**Default Commit Messages (randomly selected if none provided):**

- "Fixed the thing. You know, THAT thing. 🙄"
- "This commit is chef's kiss 👌 Your code review? Probably not."
- "Made the build green. Made the reviewers green with envy 💚"

**Default PR Titles (randomly selected if none provided):**

- "🔥 This PR is hotter than your last performance review"
- "💪 Flex on 'em: The Commit"
- "✨ Fixed everything. You're welcome."

**Or use your own!** The script now accepts runtime parameters so you can be as creative and snarky as you want! 🎨

## Examples

```bash
# Go full chaos mode with random everything
./scripts/commit.sh

# Professional but fun
./scripts/commit.sh "Add user authentication feature" "feat: User authentication with JWT"

# Maximum sass mode activated
./scripts/commit.sh "Your code could never 💅" "🔥 Roasted the competition with this PR"

# Wholesome but still cool
./scripts/commit.sh "Fixed bugs and added tests ✨" "🎯 Quality improvements incoming"
```

## Agent Instructions

When this prompt is invoked:

1. **Generate creative snarky messages** based on the changes:
   - Analyze the git diff to understand what was changed
   - Create a witty, snarky commit message that describes the changes
   - Create a spicy, attitude-filled PR title that sells the changes
   - Keep them PG-13 but make them pop! 🔥

2. **Run the commit script** with your generated messages:
   ```bash
   cd /workspaces/Demo1 && ./scripts/commit.sh "your generated commit message" "your generated PR title"
   ```

3. **Report the results** with enthusiasm and celebrate the successful commit!

**Message Style Guidelines:**
- Commit messages: Describe what was done but make it sassy
- PR titles: Sell the changes with confidence and flair
- Use emojis liberally 💅✨🔥
- Be playful but professional enough for a real repo
- Examples:
  - Commit: "Refactored the entire codebase. It's prettier than your portfolio now 💅"
  - PR: "🎨 The Great Refactoring: Making code beautiful again"
