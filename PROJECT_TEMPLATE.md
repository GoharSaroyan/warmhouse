# Project_template

This is a template for the project submission. This file's structure
mirrors the structure of the assignments. Fill it in as you work through
the solution.

# Task 1. Analysis and Planning

<aside>

To put together a document describing the current application
architecture, you can take some of the information from the company
description and the assignment conditions. That's fine.

</aside>

### 1. Description of the monolithic application's functionality

**Heating management:**

- Users can…
- The system supports…
- …

**Temperature monitoring:**

- Users can…
- The system supports…
- …

### 2. Analysis of the monolithic application's architecture

List here the main features of the current application: what
programming language is used, what database, how interaction between
components is organized, and so on.

### 3. Domain and bounded context definition

Describe here the domains you identified.

### 4. Problems of the monolithic solution

- …
- …
- …

If you believe the current solution has no problems, justify your
position.

### 5. System context visualization — C4 diagram

Add the context diagram in the C4 model here.

To add a link in the Readme.md file, you need to use Markdown syntax.
This is done like this:

```markdown
[Link text](URL)
```

Replace `Link text` with the text you want to use for the link. Instead
of `URL`, insert the address the link should lead to. For example:

```markdown
[Visit Yandex](https://ya.ru/)
```

# Task 2. Designing a Microservice Architecture

In this assignment you only need to provide the C4 model diagrams. We're
not asking you to separately describe the resulting microservices or how
you determined the interactions between the To-Be system's components. If
you prepare the C4 diagrams correctly, they will show this on their own.

**Container diagram**

Add the diagram.

**Component diagram**

Add a diagram for each of the identified microservices.

**Code diagram**

Add one diagram, or several.

# Task 3. Developing an ER Diagram

Add the ER diagram here. It should reflect the key entities of the
system, their attributes, and the type of relationships between them.

# Task 4. Creating and Documenting the API

### 1. API type

Specify which type of API you will use for interaction between the
microservices. Explain your decision.

### 2. API documentation

Attach links here to the API documentation for the microservices you
designed in the first part of the project work. Use Swagger/OpenAPI or
AsyncAPI for documentation.

# Task 5. Working with docker and docker-compose

Go to `apps`.

That's where the monolith application for working with temperature
sensors lives. The README.md there describes how to run the solution.

You need to:

1) Build a simple temperature-api application, in any programming
   language convenient for you, that returns a random temperature value
   when queried at `/temperature?location=`.

Location is the room name, sensorId is the identifier for the room name.

```
	// If no location is provided, use a default based on sensor ID
	if location == "" {
		switch sensorID {
		case "1":
			location = "Living Room"
		case "2":
			location = "Bedroom"
		case "3":
			location = "Kitchen"
		default:
			location = "Unknown"
		}
	}

	// If no sensor ID is provided, generate one based on location
	if sensorID == "" {
		switch location {
		case "Living Room":
			sensorID = "1"
		case "Bedroom":
			sensorID = "2"
		case "Kitchen":
			sensorID = "3"
		default:
			sensorID = "0"
		}
	}
```

2) The application should be packaged in Docker and added to
   docker-compose. The default port should be 8081.

3) In addition, the smart_home application requires a database — add
   settings to the docker-compose file to run postgres, specifying the
   initialization script `./smart_home/init.sql`.

To verify, you can use the Postman collection
`smarthome-api.postman_collection.json` and call:

- Create Sensor
- Get All Sensors

A different temperature value should be shown on every call.

The reviewer will check it the exact same way.
