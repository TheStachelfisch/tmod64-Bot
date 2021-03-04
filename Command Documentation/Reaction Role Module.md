#Reaction Roles Module

---
####Summary



---

| Name                                                                                                          | Example                                                                                                         | Usage
|---------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------|----------------------------------------
| **[reactionMessage&#124;reactionRole&#124;rr] <add&#124;create> &lt;messageLink> &lt;emote> &lt;role>**       | 64!rr add https://discord.com/channels/574595004064989214/574702758133760010/766724697668059137 :cow: @Tester   | Adds a reaction message
| **[sticky&#124;stickyRoles] <remove&#124;delete> &lt;messageLink>**                                           | 64!sticky remove https://discord.com/channels/574595004064989214/574702758133760010/766724697668059137          | Removes a reaction message
| **[sticky&#124;stickyRoles] <values&#124;messages>**                                                          | 64!sticky values                                                                                                | Gets all reaction messages
---
###Permissions
| Permission       | Required
| -----------------|:-------------
| User Permission  | Requires Bot Manager, Add Roles
| Guild Permission | Add Roles, Manage Roles, Add Reactions