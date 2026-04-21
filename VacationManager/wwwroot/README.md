# Hofshi – Vacation Management System (Frontend)

Hofshi is a comprehensive vacation management system designed for organizations with team-based structures.  
It provides a clear, role-based interface for managing vacation requests, team availability, and events both team and organization-wide.

This repository contains the **frontend application**.  
The backend is maintained in a separate repository.

---

## Core Concepts

### User Roles

Hofshi supports three user roles, each with clearly defined permissions:

#### Super Admin
- Create and update users
- Assign users to teams
- Manage the overall user hierarchy

#### Admin
- View vacation requests submitted by users in their team
- Approve or reject vacation requests
- Create and manage team events
- Access team vacation statistics

#### User
- Submit vacation requests
- View the status of their own vacation requests
- Edit and resubmit rejected vacation requests
- View relevant events and calendar data

---

## Calendar-Centered Interface

The primary interface of Hofshi is a **calendar view**.

- Users and Admins can see all vacation requests relevant to them
- Approved, pending, and rejected requests are visually represented
- Events and vacations are displayed together to give a clear picture of team availability

This allows both employees and team managers to understand availability at a glance.

## Summary Sidebar

Users and Admins have a sidebar to summarize relevant info and allow further action.

- Users can view, edit, or delete their own vacation requests
- Admins can view, approve, reject, or delete vacation requests in the team
- List of events in the team, and relevant events from other teams

---

## Events and Holidays

Events and Hebrew holidays are displayed on the calendar.

Admins can create **Events** associated with a specific team.

- By default, events are visible only to members of the same team
- Admins may optionally make an event visible to the entire organization
  - Useful for team outings, offsites, or events that affect availability across teams

---

## Statistics & Compliance

Admins have access to a **Statistics** modal, which provides:

- A summary chart of vacation days taken by each team member
- Filters by:
  - Current year
  - Specific month
- Visibility of employees who have taken **5 consecutive vacation days**
  - Required under Israeli labor law

---

## Project Status

This project is under active development.  
Features and UI may change if necessary.

---

## Related Repositories

- Backend repository: *(ill add this soon)*