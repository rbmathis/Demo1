# Issue Triage Workflow

This workflow shows how @issue-helper routes issues to the appropriate specialist agents.

## Issue Triage Flow

**Trigger**: New GitHub issue or issue analysis request

### Step 1: Initial Triage (@issue-helper)
```bash
@issue-helper classify this issue
```

**Issue Helper Actions**:
- Analyzes issue description
- Classifies difficulty level
- Identifies issue type (bug/feature/docs/security)
- Suggests labels
- Checks for missing information

**Routing Decisions**:

#### 🐛 Bug Report
- Hands off to @code-reviewer
- "This looks like a bug in the controller logic! Let me get @code-reviewer to analyze the code and suggest a fix."

#### 🔒 Security Issue
- **URGENT** handoff to @security-auditor
- "This is a security concern! 🚨 @security-auditor, we need you ASAP for this report!"

#### 📚 Documentation Issue
- Hands off to @doc-helper
- "This is about documentation! @doc-helper, can you help improve the docs for this feature?"

#### ⚡ Feature Request
- Hands off to @code-reviewer for architectural guidance
- "Interesting feature request! @code-reviewer, want to provide architectural guidance on implementing this?"

#### 🛠️ Build/Dependency Issue
- Hands off to @build-validator
- "@build-validator, looks like we have a dependency or build issue here!"

## Example Issue Workflows

### Bug Report Flow

```text
User reports: "Login controller throws NullReferenceException"

Issue Helper: "Ooh, let's tackle this! 🎯 This looks like a bug report.
I'm classifying this as:
- Type: Bug
- Difficulty: Moderate
- Priority: High
- Component: Authentication

@code-reviewer, can you analyze the LoginController and identify what's
causing the NullReferenceException? Line 67 looks suspicious based on the
stack trace."

---

Code Reviewer: "On it! 💕 Let me check that controller...

[analyzes code]

Found it! Line 67 is accessing User.Identity.Name without checking if User
is authenticated first. Here's the fix:

[provides code example]

But wait... I'm seeing authentication code here. @security-auditor, mind
giving this a quick security review while we're fixing it?"

---

Security Auditor: "Absolutely! 🛡️ Good catch on involving me. While fixing
the null reference, let's also:
1. Add [Authorize] attribute
2. Validate the user object
3. Add proper error handling

This will be both functional AND secure! 💪"
```

### Security Report Flow

```text
User reports: "Found exposed API keys in config file"

Issue Helper: "🚨 SECURITY ALERT! This is critical!

@security-auditor - I need you immediately! We have a potential exposed
secrets situation in the configuration files!"

---

Security Auditor: "I'm on it! 🛡️ This is CRITICAL priority.

[scans repository]

Confirmed: Found API keys in appsettings.json. Here's what we need to do
RIGHT NOW:

1. Rotate those API keys immediately
2. Move secrets to user secrets / Azure Key Vault
3. Add appsettings.json patterns to .gitignore
4. Scan git history for exposed keys

@build-validator, after we move these secrets, can you verify the build
configuration is using the secure setup?

@doc-helper, we'll need documentation on the proper secrets management
process for the team."

---

Build Validator: "On it. ✅ I'll validate the configuration setup uses
user secrets and Key Vault properly."

Doc Helper: "I'll create comprehensive docs on secrets management! 📚"
```

### Documentation Issue Flow

```text
User reports: "The API documentation is incomplete"

Issue Helper: "Perfect! 🎯 This is definitely a documentation issue.
Classification:
- Type: Documentation
- Difficulty: Easy
- Scope: API Documentation

@doc-helper, this one's all yours! The API endpoints need better
documentation."

---

Doc Helper: "Love it! 📚 Let me review the API controllers and create
comprehensive XML comments and README documentation.

[generates documentation]

Done! I've added:
- XML comments for all public API methods
- Parameter descriptions
- Response type documentation
- Example usage

@code-reviewer, want to verify the documented API behavior matches the
actual implementation?"

---

Code Reviewer: "Looking gorgeous! 💕 Documentation matches the code
perfectly. Nice work, team!"
```

## Workflow Diagram

```text
New Issue
    ↓
[@issue-helper] Triage & Classify
    ↓
  Type Detection
    ├─→ 🐛 Bug ────────→ [@code-reviewer] → [@security-auditor] (if needed)
    ├─→ 🔒 Security ───→ [@security-auditor] → (escalate appropriately)
    ├─→ 📚 Docs ───────→ [@doc-helper] → [@code-reviewer] (verify)
    ├─→ ⚡ Feature ────→ [@code-reviewer] → (architectural guidance)
    └─→ 🛠️ Build ──────→ [@build-validator] → [@code-reviewer] (if needed)
```
