<%=

' History
' - 2016/06/17 - Tojans - Created
'
' Lines starting with 3 quotes will be parsed as `Markdown` docs.

''' Several utils to include
'''
''' # COM Dependencies
''' 
''' - `Scripting.Dictionary`
'''

Class MyCache
	''' Allows you to cache items.
	''' This is a simple wrapper around `Scripting.Dictionary`
	'''
	''' # Example
	'''
	'''     Set objCache = new MyCache()
	'''
	'''	' set a value in the cache
	'''	objCache("123") = "abc"
	'''
	'''     ' get a value from the cache
	'''
	'''     val = objCache("123")
	'''
	Private objDict

	Private Sub Class_Initialize()
		Set objDict = CreateObject("Scripting.Dictionary")
	End Sub

	Private Sub Class_Terminate()
		Set objDict = Nothing
	End Sub

	Public Default Property Get Item(key)
		''' Some usefull info about the `Get`
		Item = objDict.Item(key)
	End Property

	Public Property Let Item(key, value)
        ' an irrelevant comment
		If objDict.Exists(key) Then 
			objDict.Remove(key)
		End If
		objDict.Add(key, value) 
	End Property
End Class

%>
