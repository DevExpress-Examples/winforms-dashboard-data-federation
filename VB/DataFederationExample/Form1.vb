Imports DevExpress.DashboardCommon
Imports DevExpress.DataAccess.ConnectionParameters
Imports DevExpress.DataAccess.DataFederation
Imports DevExpress.DataAccess.Excel
Imports DevExpress.DataAccess.Sql
Imports DevExpress.XtraEditors

Namespace DataFederationExample
	Partial Public Class Form1
		Inherits XtraForm

		Public Sub New()
			InitializeComponent()
			dashboardDesigner1.CreateRibbon()
			InitializeDashboard()
		End Sub

		Public Sub InitializeDashboard()
			Dim dashboard As New Dashboard()
			Dim sqliteDataSource As DashboardSqlDataSource = CreateSQLiteDataSource()
			dashboard.DataSources.Add(sqliteDataSource)
			Dim exceldataSource As DashboardExcelDataSource = CreateExcelDataSource()
			dashboard.DataSources.Add(exceldataSource)
			Dim objectDataSource As DashboardObjectDataSource = CreateObjectDataSource()
			dashboard.DataSources.Add(objectDataSource)
			Dim federatedDS_Join As DashboardFederationDataSource = CreateFederatedDataSourceJoin(sqliteDataSource, exceldataSource, objectDataSource)
			dashboard.DataSources.Add(federatedDS_Join)
			Dim federatedDS_Union As DashboardFederationDataSource = CreateFederatedDataSourceUnion(sqliteDataSource, exceldataSource)
			dashboard.DataSources.Add(federatedDS_Union)

			Dim pivot As New PivotDashboardItem()
			pivot.DataMember = "FDS-Created-by-NodeBulder"
			pivot.DataSource = federatedDS_Join
			pivot.Rows.AddRange(New Dimension("CategoryName"), New Dimension("ProductName"))
			pivot.Columns.Add(New Dimension("SalesPerson"))
			pivot.Values.Add(New Measure("Extended Price"))

			Dim chart As New ChartDashboardItem()
			chart.DataSource = federatedDS_Join
			chart.DataMember = "FDS-Created-by-NodeBulder"
			chart.Arguments.Add(New Dimension("SalesPerson"))
			chart.Panes.Add(New ChartPane())
			Dim theSeries As New SimpleSeries(SimpleSeriesType.Bar)
			theSeries.Value = New Measure("Score")
			chart.Panes(0).Series.Add(theSeries)

			dashboard.Items.AddRange(pivot, chart)
			dashboard.RebuildLayout()
			dashboard.LayoutRoot.Orientation = DashboardLayoutGroupOrientation.Vertical
			dashboardDesigner1.Dashboard = dashboard
		End Sub

		Private Shared Function CreateFederatedDataSourceJoin(ByVal sqliteDataSource As DashboardSqlDataSource, ByVal exceldataSource As DashboardExcelDataSource, ByVal objectDataSource As DashboardObjectDataSource) As DashboardFederationDataSource
			Dim federationDS As New DashboardFederationDataSource("Federated Data Source (JOIN)")
			Dim sqlSource As New Source("sqlite", sqliteDataSource, "SQLite Orders")
			Dim excelSource As New Source("excel", exceldataSource, "")
			Dim objectSource As New Source("SalesPersonDS", objectDataSource, "")

'			#Region "Use-API-to-create-a-query"
			Dim mainQueryCreatedByApi As New SelectNode()

			mainQueryCreatedByApi.Alias = "FDS-Created-by-API"
			Dim root As New SourceNode(sqlSource, "SQLite Orders")
			Dim excelSourceNode As New SourceNode(excelSource, "ExcelDS")
			Dim objectSourceNode As New SourceNode(objectSource, "ObjectDS")

			mainQueryCreatedByApi.Root = root
			mainQueryCreatedByApi.Expressions.Add(New SelectColumnExpression() With {.Name = "SalesPerson", .Node = objectSourceNode})
			mainQueryCreatedByApi.Expressions.Add(New SelectColumnExpression() With {.Name = "Weight", .Node = objectSourceNode})
			mainQueryCreatedByApi.Expressions.Add(New SelectColumnExpression() With {.Name = "CategoryName", .Node = excelSourceNode})
			mainQueryCreatedByApi.Expressions.Add(New SelectColumnExpression() With {.Name = "ProductName", .Node = excelSourceNode})
			mainQueryCreatedByApi.Expressions.Add(New SelectColumnExpression() With {.Name = "OrderDate", .Node = root})
			mainQueryCreatedByApi.Expressions.Add(New SelectColumnExpression() With {.Name = "ShipCity", .Node = root})
			mainQueryCreatedByApi.Expressions.Add(New SelectColumnExpression() With {.Name = "ShipCountry", .Node = root})
			mainQueryCreatedByApi.Expressions.Add(New SelectColumnExpression() With {.Name = "Extended Price", .Node = excelSourceNode})
			mainQueryCreatedByApi.SubNodes.Add(New JoinElement(excelSourceNode, JoinType.Inner, "[ExcelDS.OrderID] = [SQLite Orders.OrderID]"))
			mainQueryCreatedByApi.SubNodes.Add(New JoinElement(objectSourceNode, JoinType.Inner, "[ObjectDS.SalesPerson] = [ExcelDS.Sales Person]"))
'			#End Region

'			#Region "Use-NodedBuilder-to-create-a-query"
			Dim mainQueryCreatedByNodeBuilder As SelectNode = sqlSource.From().Select("OrderDate", "ShipCity", "ShipCountry").Join(excelSource, "[excel.OrderID] = [sqlite.OrderID]").Select("CategoryName", "ProductName", "Extended Price").Join(objectSource, "[SalesPersonDS.SalesPerson] = [excel.Sales Person]").Select("SalesPerson", "Weight").Build("FDS-Created-by-NodeBulder")
'			#End Region

			federationDS.Queries.Add(mainQueryCreatedByNodeBuilder)
			federationDS.Queries.Add(mainQueryCreatedByApi)

			federationDS.CalculatedFields.Add("FDS-Created-by-NodeBulder", "[Weight] * [Extended Price] / 100", "Score")

			federationDS.Fill(New DevExpress.Data.IParameter(){})
			Return federationDS
		End Function

		Private Shared Function CreateFederatedDataSourceUnion(ByVal sqliteDataSource As DashboardSqlDataSource, ByVal exceldataSource As DashboardExcelDataSource) As DashboardFederationDataSource
			Dim federationDS As New DashboardFederationDataSource("Federated Data Source (UNION)")
			Dim sqlSource As New Source("sqlite", sqliteDataSource, "SQLite Orders")
			Dim excelSource As New Source("excel", exceldataSource, "")

			Dim queryUnionAll As UnionNode = sqlSource.From().Select("OrderID", "OrderDate").Build("OrdersSqlite").UnionAll(excelSource.From().Select("OrderID", "OrderDate").Build("OrdersExcel")).Build("OrdersUnionAll")

			Dim queryUnion As UnionNode = sqlSource.From().Select("OrderID", "OrderDate").Build("OrdersSqlite").Union(excelSource.From().Select("OrderID", "OrderDate").Build("OrdersExcel")).Build("OrdersUnion")

			federationDS.Queries.Add(queryUnionAll)
			federationDS.Queries.Add(queryUnion)

			federationDS.Fill(New DevExpress.Data.IParameter(){})
			Return federationDS
		End Function

		Private Shared Function CreateObjectDataSource() As DashboardObjectDataSource
			Dim objectDataSource As New DashboardObjectDataSource()
			objectDataSource.Name = "ObjectDS"
			objectDataSource.DataSource = DataGenerator.Data
			objectDataSource.Fill()
			Return objectDataSource
		End Function

		Private Shared Function CreateSQLiteDataSource() As DashboardSqlDataSource
			Dim sqliteParams As New SQLiteConnectionParameters()
			sqliteParams.FileName = "Data\nwind.db"

			Dim sqlDataSource As New DashboardSqlDataSource("SQLite Data Source", sqliteParams)
			Dim selectQuery As SelectQuery = SelectQueryFluentBuilder.AddTable("Orders").SelectAllColumnsFromTable().Build("SQLite Orders")
			sqlDataSource.Queries.Add(selectQuery)
			sqlDataSource.Fill()
			Return sqlDataSource
		End Function

		Private Function CreateExcelDataSource() As DashboardExcelDataSource
			Dim excelDataSource As New DashboardExcelDataSource()
			excelDataSource.Name = "ExcelDS"
			excelDataSource.FileName = "Data\SalesPerson.xlsx"
			Dim worksheetSettings As New ExcelWorksheetSettings("Data")
			excelDataSource.SourceOptions = New ExcelSourceOptions(worksheetSettings)
			excelDataSource.Fill()
			Return excelDataSource
		End Function
	End Class
End Namespace
