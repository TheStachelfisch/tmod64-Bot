#TagModule

---

| Name                                                              | Example                           | Usage
|-------------------------------------------------------------------|-----------------------------------|----------
| **[tag&#124;tags] <add&#124;create> &lt;name> [R]&lt;content>**   | 64!tag add Hello Why Hello There  | Adds a tag
| **[tag&#124;tags] <delete&#124;remove> &lt;tagName>**             | 64!tag delete Hello               | Deletes a tag. (You can only delete your own tags)
| **[tag&#124;tags] edit** &lt;tagName> [R]&lt;newContent>          | 64!tag edit Hello Good Morning    | Edits a tag. (You can only edit your own tags)
| **[tag&#124;tags] list**                                          | 64!tag list                       | Gets all the tags

---
###Permissions
| Permission       | Required
| -----------------|:-------------
| User Permission  | **Requires Admin** (Only to delete/edit tags that aren't yours)
| Guild Permission |

######[R] = remainder