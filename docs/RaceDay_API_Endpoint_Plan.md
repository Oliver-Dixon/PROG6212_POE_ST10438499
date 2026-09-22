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

## 4. Categories

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/events/{eventId}/categories | Lists the categories (distances) offered in an event. | None | None | 200 OK - list of categories; 404 Not Found - event does not exist |
| POST | /api/events/{eventId}/categories | Adds a category to the organiser's own event. | Organiser | { categoryName, distanceKm, startTime, entryFee, maxParticipants, minimumAge } | 201 Created - new category; 400 Bad Request - invalid values; 403 Forbidden - not your event; 404 Not Found - event does not exist; 409 Conflict - category name already used in this event |
| PUT | /api/categories/{id} | Updates a category in the organiser's own event. | Organiser | { categoryName, distanceKm, startTime, entryFee, maxParticipants, minimumAge } | 200 OK - updated category; 400 Bad Request - invalid values; 403 Forbidden - not your event; 404 Not Found - category does not exist |
| DELETE | /api/categories/{id} | Deletes a category that has no enrolments. | Organiser | None | 204 No Content - category deleted; 403 Forbidden - not your event; 404 Not Found - category does not exist; 409 Conflict - category has enrolments |

## 5. Event Enrolments

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/enrolments | Enters the logged-in participant into a category of an open event. | Participant | { categoryId } | 201 Created - enrolment with race number; 400 Bad Request - event not open or registration closed; 403 Forbidden - not a Participant; 404 Not Found - category does not exist; 409 Conflict - already entered or category full |
| GET | /api/users/me/enrolments | Lists the logged-in participant's enrolments. | Participant | None | 200 OK - list of enrolments; 403 Forbidden - not a Participant |
| DELETE | /api/enrolments/{id} | Cancels the participant's own enrolment (status set to Cancelled so history is kept). | Participant | None | 204 No Content - enrolment cancelled; 403 Forbidden - not your enrolment; 404 Not Found - enrolment does not exist |
| GET | /api/events/{eventId}/enrolments | Lists everyone entered in the organiser's own event. | Organiser | None | 200 OK - list of participants; 403 Forbidden - not your event; 404 Not Found - event does not exist |

## 6. Results

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/results | Records a participant's result for the organiser's own event. | Organiser | { enrolmentId, finishStatus, finishTime, categoryPosition } | 201 Created - new result; 400 Bad Request - finish time missing for a finisher or given for a DNF; 403 Forbidden - not your event; 404 Not Found - enrolment does not exist; 409 Conflict - result already recorded |
| PUT | /api/results/{id} | Corrects a recorded result. | Organiser | { finishStatus, finishTime, categoryPosition } | 200 OK - updated result; 400 Bad Request - invalid values; 403 Forbidden - not your event; 404 Not Found - result does not exist |
| GET | /api/events/{eventId}/results | Lists an event's results in finishing order. | None | None | 200 OK - list of results; 404 Not Found - event does not exist |
| GET | /api/users/me/results | Returns the logged-in participant's performance history. | Participant | None | 200 OK - list of past results; 403 Forbidden - not a Participant |

