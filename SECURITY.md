# Security Policy

## Reporting a Vulnerability

If you discover a security vulnerability in DotnetNiger, please report it by emailing the maintainers. Do **not** open a public GitHub issue.

Please include:

- Description of the vulnerability
- Steps to reproduce
- Affected endpoints/components
- Potential impact

We will acknowledge receipt within 48 hours and provide a timeline for a fix.

## Best Practices

- All API traffic must go through the Gateway
- JWT tokens are used for authentication (not cookies)
- CORS is restricted to known origins in production
- Rate limiting is applied on sensitive endpoints (auth, newsletter, upload)
- 2FA is available for all user accounts
- GDPR compliance: data export, consent tracking, and right to be forgotten
- Uploaded files are validated (type, size) and stored outside the app directory in production
- Secrets (JWT keys, SMTP passwords, OAuth secrets) are never committed — use environment variables or user secrets
