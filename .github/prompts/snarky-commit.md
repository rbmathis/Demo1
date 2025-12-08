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

### Option 1: Fully Custom with PR Body (Ultimate Control)
Provide commit message, PR title, AND custom PR description:
```bash
./scripts/commit.sh "Your commit message" "Your PR title" "Your awesome PR description explaining what changed and why"
```

### Option 2: Custom Commit & Title, Default PR Body
Provide commit message and PR title only:
```bash
./scripts/commit.sh "Your snarky commit message 🔥" "Your spicy PR title ✨"
```

### Option 3: Custom Commit, Random Everything Else
Provide just the commit message:
```bash
./scripts/commit.sh "Fixed all the things like a boss 💪"
```

### Option 4: Full Random (Original Behavior)
Let the script pick everything randomly:
```bash
./scripts/commit.sh
```

### Using with Agent Chat
When using this prompt in Agent Chat, the agent will:
- Analyze your changes
- Generate witty commit message
- Create a spicy PR title
- Write a custom PR description explaining what changed
- All messages include emojis and sass!

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

# Professional with custom description
./scripts/commit.sh "Add user authentication feature" "feat: User authentication with JWT" "Implemented JWT-based authentication with refresh tokens. Users can now securely log in and maintain sessions."

# Maximum sass mode activated
./scripts/commit.sh "Your code could never 💅" "🔥 Roasted the competition with this PR" "Refactored the entire authentication system to be more secure and performant. Previous implementation? We don't talk about that."

# Just commit and title, default body
./scripts/commit.sh "Fixed bugs and added tests ✨" "🎯 Quality improvements incoming"
```

**Note:** The PR body will ALWAYS include the required sections:
- `## ✅ Quality Checks` - Automatically filled with build/test results
- `## 🎪 Side Effects` - Either your custom text or default sass

## Agent Instructions

When this prompt is invoked:

1. **Analyze the changes** to understand what was modified:
   - Check git diff to see file changes
   - Identify the scope and nature of modifications
   - Note any patterns or significant updates

2. **Generate three creative messages**:

   **A) Commit Message:**
   - Describe what was changed in a snarky, witty way
   - Keep it concise but descriptive
   - Add relevant emojis
   - Example: "Refactored auth system. Now it's actually secure 🔒"

   **B) PR Title:**
   - Create a bold, attention-grabbing title
   - Sell the changes with confidence
   - Make it spicy but professional
   - Example: "🔥 Security Fortress: Auth system that would make Fort Knox jealous"

   **C) PR Description:**
   - Write 1-3 paragraphs explaining WHAT changed and WHY
   - Include technical details but keep it readable
   - Add personality and sass
   - Mention any breaking changes or important notes
   - Example: "Completely overhauled the authentication system. The old implementation was... let's call it 'optimistic' about security. Now we've got JWT tokens, refresh token rotation, and proper session management. Your users' data is safe, and you can sleep at night."

3. **Run the commit script** with all three generated messages:
   ```bash
   cd /workspaces/Demo1 && ./scripts/commit.sh "commit message" "PR title" "PR description"
   ```

4. **Report the results** with enthusiasm and celebrate!

**Required Sections:**
The script automatically appends these sections to every PR:
- `## ✅ Quality Checks` - Shows build, test, coverage, and runtime status
- `## 🎪 Side Effects` - Default sass or can be customized in description

**Message Style Guidelines:**
- Use emojis liberally 💅✨🔥
- Be playful but professional enough for a real repo
- Make boring changes sound exciting
- Be specific about what changed
- Keep it PG-13
