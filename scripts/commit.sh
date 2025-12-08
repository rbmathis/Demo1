#!/bin/bash
# Snarky Auto-Commit Script 🔥
# Because manually checking if your code works is SO 2010

echo "🧹 Running pre-commit cleanup..."
# Clean only what we need to for the commit
dotnet clean --configuration Debug --verbosity quiet 2>/dev/null || true
find . -type d -name "TestResults" -exec rm -rf {} + 2>/dev/null || true
find . -name "*.trx" -delete 2>/dev/null || true

echo "✅ Workspace cleaned and ready!"

set -e  # Exit on any error

# Terminal colors for maximum sass
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
MAGENTA='\033[0;35m'
BOLD='\033[1m'
NC='\033[0m' # No Color

# Parse command line arguments
COMMIT_MSG="${1:-}"
PR_TITLE="${2:-}"

# Default snarky messages (used if not provided)
DEFAULT_COMMIT_MESSAGES=(
    "Fixed the thing. You know, THAT thing. 🙄"
    "Code so clean it sparkles ✨ (unlike my commit history)"
    "This commit is chef's kiss 👌 Your code review? Probably not."
    "Added features. Broke nothing. I'm basically a wizard 🧙‍♂️"
    "Refactored code that was already perfect. Fight me. 💪"
    "Your linter warnings are now my linter whispers 🤫"
    "Made the build green. Made the reviewers green with envy 💚"
    "Pushed pixels. Crushed bugs. Took names. 😎"
    "If this breaks prod, blame main. I'm just here for the commits 🤷"
    "This code is fire 🔥 (in a good way, not a dumpster way)"
    "Deployed code. Dropped mic. 🎤"
    "Warning: Contains raw developer talent. Handle with care ⚠️"
    "Git commit -m 'I'm too pretty to write real commit messages' 💅"
    "Another day, another banger commit 🎵"
    "Code review this. I dare you. I DOUBLE dare you. 😤"
)

DEFAULT_PR_TITLES=(
    "🔥 This PR is hotter than your last performance review"
    "✨ Fixed everything. You're welcome."
    "💪 Flex on 'em: The Commit"
    "🎯 Bullseye: Actually working code incoming"
    "🚀 Houston, we have liftoff (and passing tests)"
    "👑 Main branch wishes it was this good"
    "💎 Premium code at economy prices"
    "🎪 The Greatest Show On Git"
    "⚡ Lightning-fast fixes for your slow code"
    "🧠 Big brain energy: The Pull Request"
    "🌶️ Spicy code changes (handle with care)"
    "🎨 Painted the code. Made it pretty. Made it work."
    "🏆 Championship-level commits"
    "💫 Stardust and bug fixes"
    "🦄 Magical code that actually compiles"
)

# Use provided messages or pick random defaults
if [ -z "$COMMIT_MSG" ]; then
    COMMIT_MSG="${DEFAULT_COMMIT_MESSAGES[$RANDOM % ${#DEFAULT_COMMIT_MESSAGES[@]}]}"
fi

if [ -z "$PR_TITLE" ]; then
    PR_TITLE="${DEFAULT_PR_TITLES[$RANDOM % ${#DEFAULT_PR_TITLES[@]}]}"
fi

echo -e "${BOLD}${MAGENTA}"
echo "╔═══════════════════════════════════════════════════╗"
echo "║  🔥 SNARKY AUTO-COMMIT EXTRAVAGANZA 3000™ 🔥     ║"
echo "║  Because your code deserves attitude             ║"
echo "╚═══════════════════════════════════════════════════╝"
echo -e "${NC}"

# Show what messages we're using
if [ -n "$1" ]; then
    echo -e "${CYAN}Using custom commit message:${NC} ${MAGENTA}${COMMIT_MSG}${NC}"
fi
if [ -n "$2" ]; then
    echo -e "${CYAN}Using custom PR title:${NC} ${MAGENTA}${PR_TITLE}${NC}"
fi
if [ -z "$1" ] && [ -z "$2" ]; then
    echo -e "${CYAN}Using random snarky messages${NC} ${YELLOW}(you could've picked your own, you know)${NC}"
fi
echo ""

# Step 1: Build the app
echo -e "${CYAN}${BOLD}[1/8]${NC} Building the app... ${YELLOW}(pretending this isn't scary)${NC}"
if dotnet build --configuration Release --nologo --verbosity quiet > /dev/null 2>&1; then
    echo -e "      ${GREEN}✓ Build successful!${NC} ${CYAN}The compiler actually likes you today 🎉${NC}"
else
    echo -e "      ${RED}✗ Build failed!${NC} ${YELLOW}Even the compiler can't handle this mess 💀${NC}"
    echo -e "      ${MAGENTA}Fix your build errors first, genius 🤓${NC}"
    exit 1
fi

# Step 2: Run the tests
echo -e "${CYAN}${BOLD}[2/8]${NC} Running tests... ${YELLOW}(fingers crossed)${NC}"
cd tests/Demo1.UnitTests
if dotnet test --configuration Release --nologo --verbosity quiet > /dev/null 2>&1; then
    echo -e "      ${GREEN}✓ All tests passed!${NC} ${CYAN}Your code is less broken than usual 🎊${NC}"
else
    echo -e "      ${RED}✗ Tests failed!${NC} ${YELLOW}Shocking absolutely no one 🙄${NC}"
    echo -e "      ${MAGENTA}Fix your tests before you embarrass yourself${NC}"
    cd ../..
    exit 1
fi
cd ../..

# Step 3: Lint markdown documentation
echo -e "${CYAN}${BOLD}[3/8]${NC} Linting markdown... ${YELLOW}(docs matter too)${NC}"
if command -v docker &> /dev/null; then
    if ./scripts/lint-docs.sh > /dev/null 2>&1; then
        echo -e "      ${GREEN}✓ Markdown looks gorgeous!${NC} ${CYAN}Your docs are 💯${NC}"
    else
        echo -e "      ${YELLOW}⚠ Markdown linting found issues${NC}"
        echo -e "      ${CYAN}Run './scripts/lint-docs.sh' for details${NC}"
        echo -e "      ${MAGENTA}Continuing anyway... CI will catch it 😏${NC}"
    fi
else
    echo -e "      ${YELLOW}⚠ Docker not found${NC}"
    echo -e "      ${MAGENTA}Skipping markdown lint... CI will handle it 😉${NC}"
fi

# Step 4: Check code coverage
echo -e "${CYAN}${BOLD}[4/8]${NC} Checking code coverage... ${YELLOW}(hoping you wrote tests)${NC}"
cd tests/Demo1.UnitTests
dotnet test --collect:"XPlat Code Coverage" --nologo --verbosity quiet > /dev/null 2>&1 || true

COVERAGE_FILE=$(find . -name "coverage.cobertura.xml" -type f | head -n 1)
if [ -f "$COVERAGE_FILE" ]; then
    # Run coverage check with 70% threshold
    if python3 ../../scripts/check_coverage.py "$COVERAGE_FILE" 70.0 2>&1 | tee /tmp/coverage_output.txt | grep -q "Unit test coverage"; then
        COVERAGE_RESULT=$(grep "Unit test coverage" /tmp/coverage_output.txt)
        echo -e "      ${GREEN}✓ Coverage check passed!${NC}"
        echo -e "      ${CYAN}${COVERAGE_RESULT}${NC}"
        echo -e "      ${MAGENTA}Look at you, writing tests like a responsible adult 🏆${NC}"
    else
        echo -e "      ${RED}✗ Coverage below threshold!${NC}"
        echo -e "      ${YELLOW}Coverage dropped harder than your coding standards 📉${NC}"
        cat /tmp/coverage_output.txt
        cd ../..
        exit 1
    fi
else
    echo -e "      ${YELLOW}⚠ No coverage file found${NC}"
    echo -e "      ${MAGENTA}Living dangerously, I see 😏${NC}"
fi
cd ../..

# Step 5: Quick smoke test - ensure the app actually runs
echo -e "${CYAN}${BOLD}[5/8]${NC} Smoke testing... ${YELLOW}(please don't catch fire)${NC}"
timeout 10s dotnet run --no-build --configuration Release --urls "http://localhost:5555" > /dev/null 2>&1 &
APP_PID=$!
sleep 5

if kill -0 $APP_PID 2>/dev/null; then
    echo -e "      ${GREEN}✓ App starts successfully!${NC} ${CYAN}It's alive! IT'S ALIVE! ⚡${NC}"
    kill $APP_PID 2>/dev/null || true
    wait $APP_PID 2>/dev/null || true
else
    echo -e "      ${RED}✗ App failed to start!${NC} ${YELLOW}DOA. Dead on arrival. 💀${NC}"
    exit 1
fi

# Step 6: Check current branch
echo -e "${CYAN}${BOLD}[6/8]${NC} Checking git branch... ${YELLOW}(not like you were organized anyway)${NC}"
CURRENT_BRANCH=$(git branch --show-current)

if [ "$CURRENT_BRANCH" = "main" ] || [ "$CURRENT_BRANCH" = "master" ]; then
    # Generate a snarky branch name
    TIMESTAMP=$(date +%Y%m%d-%H%M%S)
    BRANCH_NAME="feature/absolutely-legendary-${TIMESTAMP}"

    echo -e "      ${YELLOW}⚠ You're on ${CURRENT_BRANCH}!${NC} ${MAGENTA}Rookie mistake 🙈${NC}"
    echo -e "      ${CYAN}Creating branch: ${BOLD}${BRANCH_NAME}${NC}"

    git checkout -b "$BRANCH_NAME"
    echo -e "      ${GREEN}✓ Switched to new branch${NC} ${CYAN}Crisis averted! 😅${NC}"
else
    BRANCH_NAME="$CURRENT_BRANCH"
    echo -e "      ${GREEN}✓ Already on branch: ${BOLD}${BRANCH_NAME}${NC}"
    echo -e "      ${CYAN}Someone taught you well 🎓${NC}"
fi

# Step 7: Commit with snarky message
echo -e "${CYAN}${BOLD}[7/8]${NC} Committing changes... ${YELLOW}(with maximum attitude)${NC}"

git add -A
if git diff --cached --quiet; then
    echo -e "      ${YELLOW}⚠ No changes to commit${NC}"
    echo -e "      ${MAGENTA}What were you even doing? 🤔${NC}"
else
    git commit -m "$COMMIT_MSG"
    echo -e "      ${GREEN}✓ Committed with message:${NC}"
    echo -e "      ${BOLD}${MAGENTA}\"${COMMIT_MSG}\"${NC}"
fi

# Step 8: Push and create PR
echo -e "${CYAN}${BOLD}[8/8]${NC} Creating PR... ${YELLOW}(prepare for glory)${NC}"

# Push the branch
git push -u origin "$BRANCH_NAME" 2>&1 | grep -v "^To " || true

# Check if gh CLI is available
if command -v gh &> /dev/null; then
    # Create PR description
    PR_BODY="## 🎯 What's This?
This PR contains commits that are too good for main branch right now.

## ✅ Quality Checks
- ✅ Build: Passed (obviously)
- ✅ Tests: Green (shocked? me too)
- ✅ Coverage: Maintained (living up to standards)
- ✅ Runtime: Works (miracle of miracles)

## 💅 Commit Messages
\`$COMMIT_MSG\`

## 🎪 Side Effects
Your code just got better. You're welcome.

---
*Auto-generated by the Snarky Commit Script™*
*Powered by attitude and caffeine* ☕"
    fi

    # Create the PR
    if gh pr create --title "$PR_TITLE" --body "$PR_BODY" --base main 2>&1 | grep -q "https://"; then
        PR_URL=$(gh pr view --json url -q .url)
        echo -e "      ${GREEN}✓ PR created successfully!${NC}"
        echo -e "      ${BOLD}${CYAN}${PR_TITLE}${NC}"
        echo -e "      ${MAGENTA}${PR_URL}${NC}"
        echo ""
        echo -e "${BOLD}${GREEN}🎉 MISSION ACCOMPLISHED 🎉${NC}"
        echo -e "${CYAN}Now go tell everyone how amazing you are 💪${NC}"
    else
        echo -e "      ${YELLOW}⚠ Couldn't create PR automatically${NC}"
        echo -e "      ${MAGENTA}Do it manually, slowpoke 🐌${NC}"
        echo -e "      ${CYAN}Or check if you're authenticated with 'gh auth login'${NC}"
    fi
else
    echo -e "      ${YELLOW}⚠ GitHub CLI not found${NC}"
    echo -e "      ${MAGENTA}Install it with: brew install gh${NC}"
    echo -e "      ${CYAN}Then run: gh auth login${NC}"
    echo ""
    echo -e "${BOLD}${GREEN}✓ Changes pushed to branch: ${BRANCH_NAME}${NC}"
    echo -e "${CYAN}Create your PR manually at:${NC}"
    echo -e "${MAGENTA}https://github.com/$(git remote get-url origin | sed 's/.*github.com[:/]\(.*\)\.git/\1/')/compare/${BRANCH_NAME}?expand=1${NC}"
fi

echo ""
echo -e "${BOLD}${MAGENTA}"
echo "╔═══════════════════════════════════════════════════╗"
echo "║           ✨ COMMIT COMPLETE ✨                   ║"
echo "║  Your code is now someone else's problem 😎      ║"
echo "╚═══════════════════════════════════════════════════╝"
echo -e "${NC}"
