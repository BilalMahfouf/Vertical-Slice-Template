# Authentication System Architecture

## Overview

The Veterinary Application uses a **JWT (JSON Web Token) based authentication system** with refresh token rotation for secure user authentication and session management. The system implements a modern token-based approach where short-lived access tokens are paired with longer-lived refresh tokens.

---

## High-Level Flow

### 1. **User Login Flow**

```
User Request: POST /api/v1/auth/login
    ↓
[Backend Process]
    ├─ Query database for user by email
    ├─ Validate user exists
    ├─ Hash incoming password and compare with stored hash
    ├─ Generate JWT access token (15 minutes)
    ├─ Generate refresh token (7 days)
    ├─ Store refresh token in database (UserSession)
    └─ Set refresh token as HTTP-Only cookie
    ↓
Response: JWT Access Token to client
```

**Key Steps:**
1. **User Lookup**: Find the user in the database by their email address
2. **Password Verification**: Compare the provided password with the stored hashed password using Argon2
3. **Token Generation**: 
   - Access Token: Short-lived JWT (default 15 minutes)
   - Refresh Token: Long-lived secure token (7 days)
4. **Session Tracking**: Save the refresh token in the database to track user sessions
5. **Secure Storage**: Set refresh token as HTTP-Only cookie (prevents JavaScript access)

---

### 2. **Authenticated Request Flow**

```
Client Request: GET /api/v1/auth/me (with Authorization header)
    ↓
[Middleware: JWT Validation]
    ├─ Extract JWT from Authorization header
    ├─ Validate JWT signature using secret key
    ├─ Verify token hasn't expired
    ├─ Verify issuer and audience match
    └─ Extract user claims (User ID, Name, etc.)
    ↓
[Endpoint Handler]
    ├─ Access user information via claims
    ├─ Query database for full user details
    └─ Return user info
    ↓
Response: User data
```

**Key Points:**
- JWT carries user identity information as **claims** (User ID, Name, roles)
- No database lookup needed for token validation—signature verification is cryptographic
- Tokens are stateless; validation happens on every request

---

### 3. **Token Refresh Flow**

```
Access Token Expired (or about to expire)
    ↓
Client Request: POST /api/v1/auth/refresh-token
    (Refresh token automatically sent via HTTP-Only cookie)
    ↓
[Backend Process]
    ├─ Extract refresh token from cookie
    ├─ Query database to find matching UserSession
    ├─ Validate session exists
    ├─ Check if refresh token hasn't expired (7 days)
    ├─ Generate new access token
    ├─ Rotate refresh token (generate new one)
    ├─ Update session in database with new refresh token
    └─ Set new refresh token as HTTP-Only cookie
    ↓
Response: New access token
```

**Security Feature - Token Rotation:**
- Each refresh extends the session with a brand new refresh token
- Old refresh tokens become invalid after use
- Prevents token replay attacks

---

## System Architecture Components


### Database Layer

#### **UserSession Entity**
Stores refresh token information:
- **UserId**: References the user
- **Token**: The refresh token value
- **TokenType**: Identifies token as "Refresh"
- **ExpiresAt**: When this session/token expires

Purpose: Enables server-side session revocation and token management

---

## Request-Response Lifecycle

### Example: Login Request

**Request:**
```json
POST /api/v1/auth/login
{
  "email": "doctor@clinic.com",
  "password": "SecurePassword123"
}
```

**Processing:**
1. Endpoint receives `LoginCommand` with email and password
2. Handler queries `Users` table: `WHERE Email = 'doctor@clinic.com'`
3. User found → extract hashed password
4. Password hasher verifies: `Argon2Verify(input_password, stored_hash)`
5. If valid:
   - JWT Provider creates access token (15 min)
   - JWT Provider creates refresh token (random 32 bytes)
   - New `UserSession` record created in database
   - Response sets cookie: `refreshToken=<token>; HttpOnly; Secure; SameSite=None`
6. Return access token in JSON response

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

Client receives access token and automatically gets refresh token cookie from browser.

---

### Example: Protected Resource Request

**Request:**
```
GET /api/v1/auth/me
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Processing:**
1. JWT Bearer middleware intercepts request
2. Extracts token from `Authorization` header
3. Validates:
   - Signature matches secret key ✓
   - Token hasn't expired ✓
   - Issuer/Audience correct ✓
4. Extract claims: `UserId = "f47ac10b-58cc-4372-a567-0e02b2c3d479"`
5. Endpoint handler gets `User ID` from claims
6. Query `GetUserById` with extracted ID
7. Return user details

**Response:**
```json
{
  "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "firstName": "John",
  "lastName": "Doe",
  "email": "doctor@clinic.com",
  "role": "Doctor"
}
```

---

### Example: Token Refresh

**Request:**
```
POST /api/v1/auth/refresh-token
Cookie: refreshToken=abc123...def456
```

**Processing:**
1. Extract refresh token from cookie
2. Query `UserSessions` table for matching token
3. Session found → check `ExpiresAt > DateTime.Now`
4. If valid:
   - Generate new access token (renewed 15 min timer)
   - Generate new refresh token
   - Update session record with new refresh token
   - Response: Set new cookie with updated refresh token
5. Return new access token

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
Set-Cookie: refreshToken=new_token...; HttpOnly; Secure; SameSite=None; Expires=...
```

---

## Security Design

### **Access Token (JWT)**
- **Short-lived** (15 minutes) reduces exposure if compromised
- **Stateless** validation (no database lookup) for performance
- **Signed** with secret key—cannot be forged
- Sent in clear text but only over HTTPS

### **Refresh Token**
- **Long-lived** (7 days) for user convenience
- **HTTP-Only cookie** prevents JavaScript access (XSS protection)
- **Stored in database** enables server-side revocation
- **Rotated on each use** prevents replay attacks
- **Secure & SameSite None** flags prevent CSRF

### **Password Storage**
- **Argon2 hashing** with salt—modern cryptographic algorithm
- Resistant to brute force and GPU attacks
- Each login performs fresh verification

### **Token Validation**
- **Cryptographic signature** verification (cannot tamper)
- **Expiration check** prevents old tokens
- **Issuer/Audience validation** ensures token is for this app

---

## Error Handling

**Common Authentication Errors:**

1. **User Not Found**
   - Email doesn't exist in database
   - Response: 404 Not Found

2. **Invalid Credentials**
   - Password doesn't match hash
   - Response: 401 Unauthorized

3. **Expired Refresh Token**
   - Session refresh token has exceeded 7-day window
   - Response: 401 Unauthorized → Must login again

4. **Invalid JWT in Header**
   - Token signature doesn't verify
   - Token has expired
   - Response: 401 Unauthorized


## Summary

The authentication system balances **security** with **user experience**:

- **Access tokens** are short-lived and stateless (performance)
- **Refresh tokens** are stored and rotated (security & session control)
- **Passwords** are hashed with modern algorithms (protection at rest)
- **JWTs** are cryptographically signed (protection in transit)
- **HTTP-Only cookies** protect refresh tokens (XSS prevention)

This design enables secure single sign-on, token refresh without re-login, and server-side session management for compliance and security monitoring.
