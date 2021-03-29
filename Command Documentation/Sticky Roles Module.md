# Sticky Roles Module

---
#### Summary

Commands to add|remove|list sticky roles.
Sticky roles are roles that will be reassigned after a user left and joins again

---

| Name                                                        | Example                                   | Usage
|-------------------------------------------------------------|-------------------------------------------|------------------------------------------
| **[sticky&#124;stickyRoles] add <@Role&#124;roleId>**       | 64!sticky add @Support Staff              | Adds a role to the sticky roles
| **[sticky&#124;stickyRoles] remove <@Role&#124;roleId>**    | 64!sticky remove @Support Staff           | Removes a role from the Sticky roles
| **[sticky&#124;stickyRoles] remove <@User;userId>**         | 64!sticky remove @TheStachelfisch#0395    | Removes a user from the Sticky roles list
| **[sticky&#124;stickyRoles] <values&#124;roles>**           | 64!sticky values                          | Gets all stickied roles
---
### Permissions
| Permission       | Required
| -----------------|:-------------
| User Permission  | Requires Bot Manager
| Guild Permission | Add Roles, Manage Roles
