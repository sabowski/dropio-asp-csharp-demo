<%@ Page Language="C#" Inherits="dropasp.Default" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html>
<head runat="server">
	<title>Default</title> 

</head>
<body id="body1" runat="server">
	<form id="form1" runat="server">
	
		<table>
			<tr>
				<td>Api Key</td>
				<td><asp:TextBox id="apiKeyTextBox" Width="300px" runat="server" /></td>
			</tr>
			<tr>
				<td>Api Secret</td>
				<td><asp:TextBox id="apiSecretTextBox" Width="300px" runat="server" /></td>
			</tr>
			<tr>
				<td colspan="2">
					<p>
						<asp:Button id="findDropsButton" runat="server" Text="Find Drops" OnClick="findDropsButton_Click"/>
						<asp:DropDownList id="dropsList" runat="server" AutoPostBack="true" OnSelectedIndexChanged="getDropButton_Click"/>
						<asp:Button id="getDropButton" runat="Server" OnClick="getDropButton_Click" text="Get Drop Info"/>
						<asp:Button id="createDropButton" runat="server" Text="Create a Drop" OnClick="createDropButton_Click" Visible="false" />
					</p>
				</td>
			</tr>
			
		</table>
		
		<asp:MultiView id="multiView1" runat="server" ActiveViewIndex="0">
		
			<asp:View id="emptyView" runat="server">
				
			</asp:View>
			
			<asp:View id="dropView" runat="server">
				<asp:Table id="dropViewTable" runat="server" GridLines="Both">
					<asp:TableRow>
						<asp:TableCell>Name</asp:TableCell>
						<asp:TableCell id="dropName" runat="server"></asp:TableCell>
					</asp:TableRow>
					
					<asp:TableRow>
						<asp:TableCell>Email</asp:TableCell>
						<asp:TableCell id="dropEmail" runat="server"></asp:TableCell>
					</asp:TableRow>
					
					<asp:TableRow>
						<asp:TableCell># of Assets</asp:TableCell>
						<asp:TableCell id="dropNumberOfAssets" runat="server"/>
					</asp:TableRow>
					
					<asp:TableRow>
						<asp:TableCell>Current Size</asp:TableCell>
						<asp:TableCell id="dropCurrentSize" runat="server"/>
					</asp:TableRow>
					
					<asp:TableRow>
						<asp:TableCell>Max Size</asp:TableCell>
						<asp:TableCell id="dropMaxSize" runat="server"/>
					</asp:TableRow>
					
					<asp:TableRow>
						<asp:TableCell>Description</asp:TableCell>
						<asp:TableCell id="dropDescription" runat="server"/>
					</asp:TableRow>
					
					<asp:TableRow>
						<asp:TableCell>Chat Password</asp:TableCell>
						<asp:TableCell id="dropChatPassword" runat="server"/>
					</asp:TableRow>
				</asp:Table>
				
				<p><asp:Button runat="server" id="viewAssetsButton" text="View Assets" OnClick="viewAssetsButton_Click"/>
				<asp:DropDownList runat="server" id="assetsPage" />
				<asp:DropDownList runat="server" id="assetsOrder">
					<asp:ListItem Value="oldest" Selected="true">Oldest First</asp:ListItem>
					<asp:ListItem Value="newest">Newest First</asp:ListItem>
				</asp:DropDownList></p>
				
				
				<p><asp:Button runat="server" id="addAssetButton" text="Add Asset" OnClick="addAssetButton_Click" /></p>
				
				<p><asp:Button runat="server" id="emptyDropButton" text="Empty Drop" OnClick="emptyDropButton_Click"/></p>
				
				<p><asp:Button runat="server" id="destroyDropButton" text="Destroy Drop" OnClick="destroyDropButton_Click"/></p>
				
				
				
			</asp:View>
			
			<asp:View id="assetListView" runat="server">
			
			</asp:View>
			
			<asp:View id="singleAssetView" runat="server">
			
			</asp:View>
			
			<asp:View id="addAssetView" runat="server">
	
				<p><h3>Add asset to drop <asp:Label id="dropNameLabel" runat="server"/></h3></p>
	<!--			<p><asp:FileUpload runat="server" id="fileToUpload"></asp:FileUpload></p> -->
				<p>Description <asp:TextBox id="assetDescriptionTextBox" width="400" runat="server"/></p>
				<p>Run Default conversions? <asp:CheckBox id="assetDefaultConversionCheckBox" Checked="false" runat="server"/></p>
				<p>Pingback URL <asp:TextBox id="assetPingbackUrlTextBox" Width="400" runat="server"/></p>
				<p>Output Locations <asp:TextBox id="assetOutputLocationsTextBox" width="400" runat="server"/></p>
	<!--			<p><asp:Button runat="server" id="fileUploadButton" text="Add File to Drop" onclick="fileUploadButton_Click"/></p> -->
				<p><input id="file" name="file" type="file" runat="server" /></p>
				
				
			</asp:View>
			
			<asp:View id="createDropView" runat="server">
				Create Drop:<br /> 
				Name<asp:TextBox id="createDropName" runat="server"/><br />
				Description<asp:TextBox id="createDropDescription" runat="server" /><br />
				Email Key<asp:TextBox id="createDropEmailKey" runat="server" /><br />
				Max Size<asp:TextBox id="createDropMaxSize" runat="server" /><br />
				Password<asp:TextBox id="createDropChatPassword" runat="server" /><br />
				<asp:Button id="createDropGo" runat="server" Text="Make it so" OnClick="createDropGo_Click"/>
			
			</asp:View>
			
			<asp:View id="deleteAssetView" runat="server">
				<asp:Label id="areYouSureLabel" runat="server" />
			</asp:View>
			
			<asp:View id="assetGridView" runat="server">
			
				<asp:GridView id="gridview1" runat="server"
					DataSourceID="ObjectDataSource1"
					AllowPaging="false"
					AutoGenerateColumns="false"
					AllowSorting="true"
					CellPadding="4" ForeColor="#333333" GridLines="Both" >
						<Columns>
							<asp:BoundField DataField="Name" HeaderText="Name"/>
							<asp:BoundField DataField="Title" HeaderText="Title" />
							<asp:BoundField DataField="Filesize" HeaderText="Size"/>
							<asp:BoundField DataField="CreatedAt" HeaderText="Created"/>
							<asp:BoundField DataField="Id" HeaderText="Id"/>
							<asp:BoundField DataField="Type" HeaderText="Type"/>
							<asp:BoundField DataField="Description" HeaderText="Description"/>
						</Columns>

 				</asp:GridView>
 				
 				<asp:ObjectDataSource ID="ObjectDataSource1" runat="server"
 					SelectMethod="GetAllAssets"
 					TypeName="DropAssetsLogic" />
			
			</asp:View>
			
		</asp:MultiView>
		
	</form>
</body>
</html>