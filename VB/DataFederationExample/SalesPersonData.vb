﻿Namespace DataFederationExample
    Public Class SalesPersonData
        Public Property ID() As Integer
        Public Property SalesPerson() As String
        Public Property Weight() As Integer
        Public Property Checked() As Boolean
    End Class

    Public NotInheritable Class DataGenerator

        Private Sub New()
        End Sub
        Private Shared _data As List(Of SalesPersonData)

        Public Shared ReadOnly Property Data() As List(Of SalesPersonData)
            Get
                If _data Is Nothing Then
                    _data = CreateSourceData()
                End If
                Return _data
            End Get
        End Property
        Public Shared Function CreateSourceData() As List(Of SalesPersonData)
            Dim dataList As New List(Of SalesPersonData)()
            Dim salesPersons() As String = {"Andrew Fuller", "Michael Suyama", "Robert King", "Nancy Davolio", "Margaret Peacock", "Laura Callahan", "Steven Buchanan", "Janet Leverling"}
            Dim seed As Integer = CInt(Date.Now.Ticks And &HFFFFF&)
            Dim rand As New Random(seed)

            For i As Integer = 0 To salesPersons.Length - 1
                Dim record As New SalesPersonData()
                record.ID = i
                record.SalesPerson = salesPersons(i)
                record.Weight = rand.Next(0, 100)
                dataList.Add(record)
            Next i
            Return dataList
        End Function
    End Class
End Namespace