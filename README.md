WHY QUICK REGISTERED ITEMS AND APPROVAL/REJECTING ISNT AN OWN TABLE:

Quick Register:

Same table, incomplete data
Detected by checking if Name, Height, or Description is missing
Pilot can only see their own incomplete items

Approval/Rejection:

Same table, uses boolean flags
IsApproved and IsRejected columns
Extra columns store who did it, when, and why

Why this design?

Simple: Only one table to manage
Flexible: Status is just flags, easy to query
Complete history: All data stays in one place

This is a common pattern called "soft states" - instead of moving data between tables, you just set flags to indicate the current state.

Obstacles Table
┌─────────────────────────────────────────┐
│ Basic Info                              │
│  - Id, Name, Height, Description        │
│  - Geometry, RegisteredBy, RegisteredDate│
├─────────────────────────────────────────┤
│ Status (Boolean Flags)                  │
│  - IsApproved (true/false)              │
│  - IsRejected (true/false)              │
├─────────────────────────────────────────┤
│ Approval Details                        │
│  - ApprovedBy, ApprovedDate, Comments   │
├─────────────────────────────────────────┤
│ Rejection Details                       │
│  - RejectedBy, RejectedDate, Reason     │
└─────────────────────────────────────────┘

State Detection:
- Pending:   IsApproved=false AND IsRejected=false
- Approved:  IsApproved=true
- Rejected:  IsRejected=true
- Incomplete: Name or Height or Description is NULL
