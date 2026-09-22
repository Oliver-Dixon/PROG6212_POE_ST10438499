# PROG6212_POE_ST10438499

PROG6212 Portfolio of Evidence – ST10438499
    
# RaceDay – Event Management System

RaceDay is a web based event management system for the South African road running, walking and cycling community. It replaces paper forms and spreadsheets with one platform where organisers publish events and participants enter them online and track their results. This first part plans the system and builds the database, so no API code is written yet.

The system has two roles. An Organiser creates and manages events, adds categories such as a half marathon or a fun walk, sees who has entered and records the results. Organisers can only manage their own events. A Participant browses upcoming events, enters a category, can cancel their own entry, updates their profile and views their personal performance history.

The ERD is in docs/RaceDay_ERD.png. It shows eight entities, namely Roles, Users, EventTypes, Venues, Events, Categories, Enrolments and Results, with their primary keys, foreign keys and cardinality. Participants enrol in a category rather than a whole event because most events offer several distances, and the Enrolments table resolves this many to many relationship. The ERD matches the SQL script exactly, so there are no differences to explain.

The API endpoint plan is in docs/RaceDay_API_Endpoint_Plan.md. It lists 23 endpoints covering authentication, user profiles, events, categories, event enrolments and results, with the method, route, description, role, request body and expected responses for each one. The API in Part 2 will be built to match this plan.

The SQL script is in docs/RaceDay_Database.sql. It creates the RaceDayDB database and all eight tables with their keys and constraints, then adds sample data for organisers, participants, events, categories, enrolments and results. It can be run again at any time because it removes and recreates the tables first.

A GitHub Actions workflow checks the repository on every push. It confirms that the docs folder contains the ERD, the endpoint plan and the SQL script, and that this README describes both roles.
