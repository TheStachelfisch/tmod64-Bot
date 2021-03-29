# Tag Module

---
#### Summary

Commands to add|remove|edit|list tags

---

| Name                                                              | Example                           | Usage
|-------------------------------------------------------------------|-----------------------------------|----------
| **[tag&#124;tags] <add&#124;create> &lt;name> [R]&lt;content>**   | 64!tag add Hello Why Hello There  | Adds a tag
| **[tag&#124;tags] <delete&#124;remove> &lt;tagName>**             | 64!tag delete Hello               | Deletes a tag. (You can only delete your own tags)
| **[tag&#124;tags] edit &lt;tagName> [R]&lt;newContent>**          | 64!tag edit Hello Good Morning    | Edits a tag. (You can only edit your own tags)
| **[tag&#124;tags] list**                                          | 64!tag list                       | Gets all the tags
| **[tag&#124;tags] &lt;tagName>**                                  | 64!tag Hello or 64!Hello          | Gets a tag (Also works with just 64!Hello)

---
### Permissions
| Permission       | Required
| -----------------|:-------------
| User Permission  | **Requires Admin** (Only to delete/edit tags that aren't yours)
| Guild Permission |

###### [R] = remainder