
# `File :\Tools.asp`

 Several utils to include

## COM Dependencies

 - `Scripting.Dictionary`


## `Class MyCache`

 Allows you to cache items.
 This is a simple wrapper around `Scripting.Dictionary`

### Example

     Set objCache = new MyCache()

	' set a value in the cache
	objCache("123") = "abc"

     ' get a value from the cache

     val = objCache("123")


### `Public Default Property Get Item(key)`

 Some usefull info about the `Get`
