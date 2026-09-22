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

## 3. Events

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/events | Lists all published events so participants can browse upcoming races. | None | None | 200 OK - list of events |
| GET | /api/events/{id} | Returns one event with its venue and categories. | None | None | 200 OK - event details; 404 Not Found - event does not exist |
| POST | /api/events | Creates a new event for the logged-in organiser. | Organiser | { eventName, description, eventTypeId, venueId, eventDate, registrationDeadline } | 201 Created - new event with status Draft; 400 Bad Request - invalid dates or unknown type or venue; 403 Forbidden - not an Organiser |
| PUT | /api/events/{id} | Updates the organiser's own event, including its status (Draft, Open, Closed, Completed or Cancelled). | Organiser | { eventName, description, eventTypeId, venueId, eventDate, registrationDeadline, status } | 200 OK - updated event; 400 Bad Request - invalid values; 403 Forbidden - not your event; 404 Not Found - event does not exist |
| DELETE | /api/events/{id} | Deletes the organiser's own event if nobody has entered it. | Organiser | None | 204 No Content - event deleted; 403 Forbidden - not your event; 404 Not Found - event does not exist; 409 Conflict - event already has enrolments |
| GET | /api/event-types | Lists the event types (Road Running, Trail Running, Walking, Cycling) for the create-event form. | None | None | 200 OK - list of event types |
| GET | /api/venues | Lists the venues for the create-event form. | None | None | 200 OK - list of venues |