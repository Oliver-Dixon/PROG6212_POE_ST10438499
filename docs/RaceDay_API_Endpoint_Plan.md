# RaceDay - API Endpoint Plan

## 1. Authentication

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/auth/register | Creates a new Organiser or Participant account. | None | { firstName, lastName, email, password, role } | 201 Created - new user profile; 400 Bad Request - missing or invalid fields; 409 Conflict - email already registered |
| POST | /api/auth/login | Checks the email and password and returns a login token. | None | { email, password } | 200 OK - login token and user role; 400 Bad Request - missing fields; 401 Unauthorized - wrong email or password |

## 2. User Profile

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/users/me | Returns the logged-in user's profile. | Any | None | 200 OK - user profile; 401 Unauthorized - not logged in |
| PUT | /api/users/me | Updates the logged-in user's personal and emergency contact details. | Any | { firstName, lastName, phoneNumber, dateOfBirth, city, emergencyContactName, emergencyContactPhone } | 200 OK - updated profile; 400 Bad Request - invalid values; 401 Unauthorized - not logged in |

