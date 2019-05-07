using DevExpress.DashboardCommon;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.DataFederation;
using DevExpress.DataAccess.Excel;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraEditors;

namespace DataFederationExample
{
    public partial class Form1 : XtraForm
    {
        public Form1()
        {
            InitializeComponent();
            dashboardDesigner1.CreateRibbon();
            InitializeDashboard();
        }

        public void InitializeDashboard()
        {
            Dashboard dashboard = new Dashboard();
            DashboardSqlDataSource sqliteDataSource = CreateSQLiteDataSource();
            dashboard.DataSources.Add(sqliteDataSource);
            DashboardExcelDataSource exceldataSource = CreateExcelDataSource();
            dashboard.DataSources.Add(exceldataSource);
            DashboardObjectDataSource objectDataSource = CreateObjectDataSource();
            dashboard.DataSources.Add(objectDataSource);
            DashboardFederationDataSource federatedDS = CreateFederatedDataSource(sqliteDataSource, exceldataSource, objectDataSource);
            dashboard.DataSources.Add(federatedDS);

            PivotDashboardItem pivot = new PivotDashboardItem();
            pivot.DataMember = "FDS-Created-by-NodeBulder";
            pivot.DataSource = federatedDS;
            pivot.Rows.AddRange(new Dimension("CategoryName"), new Dimension("ProductName"));
            pivot.Columns.Add(new Dimension("SalesPerson"));
            pivot.Values.Add(new Measure("Extended Price"));

            ChartDashboardItem chart = new ChartDashboardItem();
            chart.DataSource = federatedDS;
            chart.DataMember = "FDS-Created-by-NodeBulder";
            chart.Arguments.Add(new Dimension("SalesPerson"));
            chart.Panes.Add(new ChartPane());
            SimpleSeries theSeries = new SimpleSeries(SimpleSeriesType.Bar);
            theSeries.Value = new Measure("Score");
            chart.Panes[0].Series.Add(theSeries);

            dashboard.Items.AddRange(pivot, chart);
            dashboard.RebuildLayout();
            dashboard.LayoutRoot.Orientation = DashboardLayoutGroupOrientation.Vertical;
            dashboardDesigner1.Dashboard = dashboard;
        }

        private static DashboardFederationDataSource CreateFederatedDataSource(DashboardSqlDataSource sqliteDataSource, DashboardExcelDataSource exceldataSource, DashboardObjectDataSource objectDataSource)
        {
            DashboardFederationDataSource federationDS = new DashboardFederationDataSource();
            Source sqlSource = new Source("sqlite", sqliteDataSource, "SQLite Orders");
            Source excelSource = new Source("excel", exceldataSource, "");
            Source objectSource = new Source("SalesPersonDS", objectDataSource, "");

            #region Use-API-to-create-a-query
            SelectNode mainQueryCreatedByApi = new SelectNode();

            mainQueryCreatedByApi.Alias = "FDS-Created-by-API";
            SourceNode root = new SourceNode(sqlSource, "SQLite Orders");
            SourceNode excelSourceNode = new SourceNode(excelSource, "ExcelDS");
            SourceNode objectSourceNode = new SourceNode(objectSource, "ObjectDS");

            mainQueryCreatedByApi.Root = root;
            mainQueryCreatedByApi.Expressions.Add(new SelectColumnExpression() { Name = "SalesPerson", Node = objectSourceNode });
            mainQueryCreatedByApi.Expressions.Add(new SelectColumnExpression() { Name = "Weight", Node = objectSourceNode });
            mainQueryCreatedByApi.Expressions.Add(new SelectColumnExpression() { Name = "CategoryName", Node = excelSourceNode });
            mainQueryCreatedByApi.Expressions.Add(new SelectColumnExpression() { Name = "ProductName", Node = excelSourceNode });
            mainQueryCreatedByApi.Expressions.Add(new SelectColumnExpression() { Name = "OrderDate", Node = root });
            mainQueryCreatedByApi.Expressions.Add(new SelectColumnExpression() { Name = "ShipCity", Node = root });
            mainQueryCreatedByApi.Expressions.Add(new SelectColumnExpression() { Name = "ShipCountry", Node = root });
            mainQueryCreatedByApi.Expressions.Add(new SelectColumnExpression() { Name = "Extended Price", Node = excelSourceNode });
            mainQueryCreatedByApi.SubNodes.Add(new JoinElement(excelSourceNode, JoinType.Inner, "[ExcelDS.OrderID] = [SQLite Orders.OrderID]"));
            mainQueryCreatedByApi.SubNodes.Add(new JoinElement(objectSourceNode, JoinType.Inner, "[ObjectDS.SalesPerson] = [ExcelDS.Sales Person]"));
            #endregion
            
            #region Use-NodedBuilder-to-create-a-query
            SelectNode mainQueryCreatedByNodeBuilder =
                sqlSource.From()
                .Select("OrderDate", "ShipCity", "ShipCountry")
                .Join(excelSource, "[excel.OrderID] = [sqlite.OrderID]")
                    .Select("CategoryName", "ProductName", "Extended Price")
                    .Join(objectSource, "[SalesPersonDS.SalesPerson] = [excel.Sales Person]")
                        .Select("SalesPerson", "Weight")
                        .Build("FDS-Created-by-NodeBulder");
            #endregion

            federationDS.Queries.Add(mainQueryCreatedByNodeBuilder);
            federationDS.Queries.Add(mainQueryCreatedByApi);

            federationDS.CalculatedFields.Add("FDS-Created-by-NodeBulder", "[Weight] * [Extended Price] / 100", "Score");

            federationDS.Fill(new DevExpress.Data.IParameter[0]);
            return federationDS;
        }

        private static DashboardObjectDataSource CreateObjectDataSource()
        {
            DashboardObjectDataSource objectDataSource = new DashboardObjectDataSource();
            objectDataSource.Name = "ObjectDS";
            objectDataSource.DataSource = DataGenerator.Data;
            objectDataSource.Fill();
            return objectDataSource;
        }

        private static DashboardSqlDataSource CreateSQLiteDataSource()
        {
            SQLiteConnectionParameters sqliteParams = new SQLiteConnectionParameters();
            sqliteParams.FileName = @"Data\nwind.db";

            DashboardSqlDataSource sqlDataSource = new DashboardSqlDataSource("SQLite Data Source", sqliteParams);
            SelectQuery selectQuery = SelectQueryFluentBuilder
                .AddTable("Orders")
                .SelectAllColumnsFromTable()
                .Build("SQLite Orders");
            sqlDataSource.Queries.Add(selectQuery);
            sqlDataSource.Fill();
            return sqlDataSource;
        }

        private DashboardExcelDataSource CreateExcelDataSource()
        {
            DashboardExcelDataSource excelDataSource = new DashboardExcelDataSource();
            excelDataSource.Name = "ExcelDS";
            excelDataSource.FileName = @"Data\SalesPerson.xlsx";
            ExcelWorksheetSettings worksheetSettings = new ExcelWorksheetSettings("Data");
            excelDataSource.SourceOptions = new ExcelSourceOptions(worksheetSettings);
            excelDataSource.Fill();
            return excelDataSource;
        }
    }
}
