# Security Check Skill

Scan for:
- Missing [Authorize] attributes
- SQL injection risks (string concatenation in queries)
- Missing input validation
- Exposed secrets in code
- Missing HTTPS enforcement
- CSRF protection ([ValidateAntiForgeryToken])
- XSS vulnerabilities

## Purpose
Identify common security vulnerabilities in .NET MVC applications.

## Security Checklist

### Authentication & Authorization
- [ ] [Authorize] attribute used on controllers/actions
- [ ] Anonymous access explicitly marked with [AllowAnonymous]
- [ ] Role-based or policy-based authorization implemented
- [ ] Authentication middleware configured in Program.cs

### Input Validation
- [ ] Model validation with data annotations
- [ ] ModelState.IsValid checked before processing
- [ ] User input sanitized before display (prevents XSS)
- [ ] File upload restrictions (size, type, content validation)

### Data Protection
- [ ] Passwords hashed (never stored plain text)
- [ ] Sensitive data encrypted at rest
- [ ] Connection strings in secure configuration
- [ ] No secrets in source code

### SQL Injection Prevention
- [ ] Parameterized queries or ORM used
- [ ] No string concatenation in SQL
- [ ] Entity Framework or Dapper used properly

### CSRF Protection
- [ ] [ValidateAntiForgeryToken] on POST actions
- [ ] @Html.AntiForgeryToken() in forms
- [ ] SameSite cookie policy configured

### HTTPS & Communication
- [ ] UseHsts() and UseHttpsRedirection() enabled
- [ ] Secure cookie settings
- [ ] CORS configured appropriately

## Red Flags to Report
- Plain text passwords
- SQL string concatenation
- eval() or similar dynamic code execution
- Exposed connection strings
- Missing input validation
- Disabled certificate validation
