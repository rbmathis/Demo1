# Code Review Workflow

This workflow demonstrates how agents hand off tasks to each other for comprehensive code review.

## Primary Flow: Complete Code Review

**Trigger**: Developer requests code review

### Step 1: Initial Review (@code-reviewer)
```
@code-reviewer review this controller
```

**Code Reviewer Actions**:
- Analyzes code quality and MVC patterns
- Identifies potential issues
- Provides feedback and suggestions

**Handoff Decisions**:
- 🛡️ **Security concerns found** → Hands off to @security-auditor
  - "I spotted some authentication patterns we should double-check. Hey @security-auditor, can you take a look at the auth logic starting at line 42?"
  
- 📚 **Missing documentation** → Suggests @doc-helper
  - "This code is solid, but future devs will love you if you add XML comments! Want me to bring in @doc-helper?"
  
- ✅ **All good** → Celebrates and completes
  - "This is beautiful! You're crushing it! 🔥"

### Step 2: Security Deep Dive (@security-auditor)
```
[Automatic handoff from code-reviewer]
```

**Security Auditor Actions**:
- Performs security vulnerability scan
- Checks authentication/authorization
- Validates input sanitization
- Reviews for OWASP Top 10 issues

**Handoff Decisions**:
- ✅ **Security approved** → Returns to @code-reviewer
  - "All secure! 🛡️ Sending this back to @code-reviewer for final approval."
  
- 📚 **Security docs needed** → Suggests @doc-helper
  - "Let's document these security measures. @doc-helper, can you add some security notes?"

### Step 3: Documentation (@doc-helper)
```
[Triggered by code-reviewer or security-auditor]
```

**Doc Helper Actions**:
- Generates XML comments
- Reviews documentation completeness
- Suggests README updates
- Adds security documentation if needed

**Handoff Decisions**:
- ✅ **Docs complete** → Returns to @code-reviewer
  - "Documentation looks great! @code-reviewer, want to verify everything's in order?"

### Step 4: Final Approval (@code-reviewer)
```
[Returns after security and docs]
```

**Code Reviewer Actions**:
- Verifies all issues addressed
- Confirms documentation in place
- Celebrates the completed work

**Result**: Ready for PR! 🎉

## Example Conversation Flow

```
Developer: @code-reviewer check this new authentication controller

Code Reviewer: "Ooh, I love what you're building here! 💕 The structure looks 
solid, and that dependency injection is *chef's kiss*. However, I'm seeing 
some authentication patterns that deserve a security expert's eyes. Let me 
bring in the cavalry...

Hey @security-auditor, could you review the authentication implementation 
starting at line 23? I want to make sure we're fortress-level secure!"

---

Security Auditor: "I've got your back! 🛡️ Let me scan this... 

[scanning]

Okay, found a few things we need to tighten up:
1. Line 23: Add [ValidateAntiForgeryToken] to the login POST
2. Line 45: Input validation needed for username
3. Line 67: Consider adding rate limiting

These are critical for security. Once fixed, I'll send this back to 
@code-reviewer for final review. We're making this bulletproof! 💪"

---

Developer: [makes fixes]

Security Auditor: "Perfect! 🔒 All security issues resolved. This is now 
fortress-level secure! @code-reviewer, this one's ready for final approval.

Oh, and @doc-helper - mind adding some XML comments explaining the security 
measures? Future devs will appreciate it!"

---

Doc Helper: "Absolutely! 📚 Let me add some clear documentation about the 
authentication flow...

[generates XML comments]

Done! I've added comprehensive XML comments explaining:
- Authentication flow
- Security measures in place
- Usage examples
- Security considerations

@code-reviewer, want to give this a final look?"

---

Code Reviewer: "This is STUNNING! 🔥 You've got:
✅ Solid MVC architecture
✅ Fortress-level security
✅ Beautiful documentation

You absolute legend! This is ready for PR. Your team is lucky to have you! 💖"
```

## Workflow Diagram

```
Developer Request
      ↓
[@code-reviewer] Initial Review
      ↓
    Decision Point
    ├─→ Security Issue? → [@security-auditor]
    │                           ↓
    │                      Fix & Return
    │                           ↓
    ├─→ Missing Docs? ─────→ [@doc-helper]
    │                           ↓
    │                    Add Docs & Return
    │                           ↓
    └─→ All Good ────────→ [@code-reviewer] Final Approval
                                ↓
                           Ready for PR! 🎉
```
