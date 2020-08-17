Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic

Namespace DataFederationExample
	Public Class SalesPersonData
		Private privateID As Integer
		Public Property ID() As Integer
			Get
				Return privateID
			End Get
			Set(ByVal value As Integer)
				privateID = value
			End Set
		End Property
		Private privateSalesPerson As String
		Public Property SalesPerson() As String
			Get
				Return privateSalesPerson
			End Get
			Set(ByVal value As String)
				privateSalesPerson = value
			End Set
		End Property
		Private privateWeight As Integer
		Public Property Weight() As Integer
			Get
				Return privateWeight
			End Get
			Set(ByVal value As Integer)
				privateWeight = value
			End Set
		End Property
		Private privateChecked As Boolean
		Public Property Checked() As Boolean
			Get
				Return privateChecked
			End Get
			Set(ByVal value As Boolean)
				privateChecked = value
			End Set
		End Property
	End Class

	Public NotInheritable Class DataGenerator
		Private Shared _data As List(Of SalesPersonData)

		Private Sub New()
		End Sub
		Public Shared ReadOnly Property Data() As List(Of SalesPersonData)
			Get
				If _data Is Nothing Then
					_data = CreateSourceData()
				End If
				Return _data
			End Get
		End Property
		Public Shared Function CreateSourceData() As List(Of SalesPersonData)
			Dim data As New List(Of SalesPersonData)()
			Dim salesPersons() As String = { "Andrew Fuller", "Michael Suyama", "Robert King", "Nancy Davolio", "Margaret Peacock", "Laura Callahan", "Steven Buchanan", "Janet Leverling" }
			Dim seed As Integer = CInt(Fix(DateTime.Now.Ticks))
			Dim rand As New Random(seed)

			For i As Integer = 0 To salesPersons.Length - 1
				Dim record As New SalesPersonData()
				record.ID = i
				record.SalesPerson = salesPersons(i)
				record.Weight = rand.Next(0, 100)
				data.Add(record)
			Next i
			Return data
		End Function
	End Class
End Namespace
