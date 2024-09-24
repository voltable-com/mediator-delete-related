# MediatR Custom Priority Handler Demo

This project demonstrates a custom implementation of the MediatR library in C#, showcasing a priority-based notification handling system.

## Overview

This program implements a custom mediator pattern using MediatR, with the following key features:

1. A `CustomMediator` class that extends MediatR's `Mediator` to handle notifications based on priority.
2. Various notification handlers that implement `IPriorityDeleteHandler` to specify their deletion priority.
3. A cascading delete operation for a case management system.

## Key Components

### CustomMediator

The `CustomMediator` class overrides the default MediatR publish strategy to order handlers based on their specified priority. Handlers with lower priority numbers are executed first.

### Notifications

- `DeleteCase`: Represents a request to delete a case.
- `DeleteCaseFolder`: Represents a request to delete a case folder.

### Handlers

1. `CaseFolder`: Handles `DeleteCase` notifications with a priority of 5.
2. `CaseDocuments`: Handles `DeleteCase` notifications with a priority of 2.
3. `CaseFolderNote`: Handles `DeleteCaseFolder` notifications with a priority of 2.
4. `CaseFolderImages`: Handles `DeleteCaseFolder` notifications with a priority of 1.

## Workflow

1. The program starts by setting up dependency injection and registering the custom mediator.
2. A `Cases` object is created and its `Delete` method is called.
3. The `Delete` method publishes a `DeleteCase` notification.
4. The `CustomMediator` processes the notification, calling handlers in order of their priority:
   - `CaseDocuments` handler (priority 2)
   - `CaseFolder` handler (priority 5)
5. The `CaseFolder` handler publishes a `DeleteCaseFolder` notification.
6. The `CustomMediator` processes the `DeleteCaseFolder` notification, calling handlers in order:
   - `CaseFolderImages` handler (priority 1)
   - `CaseFolderNote` handler (priority 2)

## Purpose

This demo showcases how to implement a custom priority-based notification system using MediatR. It's particularly useful in scenarios where certain operations need to be performed in a specific order, such as cascading deletes in a complex data model.

## Running the Program

To run the program, ensure you have the necessary dependencies (MediatR and Microsoft.Extensions.DependencyInjection) installed. The `Main` method in the `Program` class serves as the entry point and demonstrates the usage of the custom mediator.