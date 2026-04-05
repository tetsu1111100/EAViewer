

/*
 * Table Schema主說明檔
 * 請自行改接上您的實際資料庫
 */
CREATE view [dbo].[V_EA_VIEWER_TABLE]
as

select '127.0.0.1' as DbHost, 'EAViewer' as DbName, 'EA_CELL_BACKGROUND_COLOR' as TableName, 'EA Viewer Cell背景色設定' as TableDesc, 'RGB顏色' as Remark

/*================================================================================================================================================================*/

/*
 * Table Schema明細說明檔
 * 請自行改接上您的實際資料庫
 */
CREATE view [dbo].[V_EA_VIEWER_TABLE_DETAIL]
as

select 'DESKTOP-SA707TD' as DbHost ,'EAViewer' as DbName ,'EA_CELL_BACKGROUND_COLOR' as TableName ,'IndexNo' as ColName ,'遞增流水號' as ColDesc ,'' as Remark ,'Y' as PK ,'Y' as AntoIncrement ,'int' as ColType ,'N' as AllowNull ,'' as DefaultValue ,'001' as SortValue

union all

select 'DESKTOP-SA707TD' as DbHost ,'EAViewer' as DbName ,'EA_CELL_BACKGROUND_COLOR' as TableName ,'Creator' as ColName ,'建立者' as ColDesc ,'' as Remark ,'N' as PK ,'N' as AntoIncrement ,'nvarchar(255)' as ColType ,'Y' as AllowNull ,'' as DefaultValue ,'002' as SortValue

union all

select 'DESKTOP-SA707TD' as DbHost ,'EAViewer' as DbName ,'EA_CELL_BACKGROUND_COLOR' as TableName ,'CreateDate' as ColName ,'建立日期' as ColDesc ,'' as Remark ,'N' as PK ,'N' as AntoIncrement ,'nvarchar(8)' as ColType ,'Y' as AllowNull ,'' as DefaultValue ,'003' as SortValue

union all

select 'DESKTOP-SA707TD' as DbHost ,'EAViewer' as DbName ,'EA_CELL_BACKGROUND_COLOR' as TableName ,'CreateTime' as ColName ,'建立時間' as ColDesc ,'' as Remark ,'N' as PK ,'N' as AntoIncrement ,'nvarchar(6)' as ColType ,'Y' as AllowNull ,'' as DefaultValue ,'004' as SortValue

/*================================================================================================================================================================*/

/*
 * Table Schema明細背景色設定檔
 * 請自行改接上您的實際資料庫
 */
CREATE TABLE [dbo].[EA_CELL_BACKGROUND_COLOR](
	[IndexNo] [int] IDENTITY(1,1) NOT NULL,
	[Creator] [nvarchar](255) NULL,
	[CreateDate] [nvarchar](8) NULL,
	[CreateTime] [nvarchar](6) NULL,
	[DbHost] [nvarchar](255) NULL,
	[DbName] [nvarchar](255) NULL,
	[TableName] [nvarchar](255) NULL,
	[ColName] [nvarchar](255) NULL,
	[TargetColumnName] [nvarchar](255) NULL,
	[BackgroundColor] [nvarchar](255) NULL,
 CONSTRAINT [PK_EA_CELL_BACKGROUND_COLOR] PRIMARY KEY CLUSTERED 
(
	[IndexNo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO



