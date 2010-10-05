using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;  
using Dropio.Core;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Data;
using System.Configuration;

public static class GlobalDropRef
{
	public static Drop drop { get; set; }
	//public static int assetPageCount { get; set;}
	public static int assetsPageSelectedValue { get; set; }
	public static Order assetOrder {get; set;}
	public static Page page {get; set;}
	
}

public static class TimeKeeper
{
	public static DateTime timeIn{ get; set; }
	public static DateTime timeOut{ get; set; }
	
	public static void TimeIn ()
	{
		TimeKeeper.timeOut = new DateTime ();
		TimeKeeper.timeIn = DateTime.Now;
	}

	public static void TimeOut (Page page, string message)
	{
		TimeKeeper.timeOut = DateTime.Now;
		TimeSpan ts = TimeKeeper.timeOut - TimeKeeper.timeIn;
		
		Console.WriteLine ( message + "(" + ts.TotalMilliseconds.ToString () + "ms )");
		page.Response.Write ("<p>" + message + "(" + ts.TotalMilliseconds.ToString () + "ms )</p>");
	}
}

namespace dropasp
{

	public partial class Default : System.Web.UI.Page
	{
		
		public virtual void Page_Load (object sender, EventArgs args)
		{			
			if(dropsList.Items.Count == 0)
			{
				// if we haven't grabbed drops yet, we don't want to see these controls
				dropsList.Visible = false;
				getDropButton.Visible = false;
				
				// make my life easier, fill in the api key and find the drops
				apiKeyTextBox.Text = "606cd9826f125130709c0a2eb342d72788624f6a";
				findDropsButton_Click( this, null );
			}
		}

		// get a list of drops associtated with an api key and propagate the DropDownList with the drop names
		public virtual void findDropsButton_Click (object sender, EventArgs args)
		{
			// get the the api keys from the form
			ServiceProxy.Instance.ServiceAdapter.ApiKey = apiKeyTextBox.Text;
			ServiceProxy.Instance.ServiceAdapter.ApiSecret = apiSecretTextBox.Text;
			
			// assume there is more than one page of results
			bool moreDrops = true;
			int resultsPage = 1;
			
			// clear the DropDownList holding the list of drops
			dropsList.Items.Clear ();
			
			// get drops
			List<Drop> drops = new List<Drop> ();
			try
			{
				TimeKeeper.TimeIn ();
				drops = Drop.FindManagerDrops (resultsPage);
				TimeKeeper.TimeOut ( this.Page, "Drop.FindManagerDrops page 1: ");
				// this gets set to visible here because we still want to be able to add drops even if the drop is empty
				createDropButton.Visible = true;
				
			}
			catch (ServiceException exc)
			{
				Response.Write (exc.serviceError + ": " + exc.serviceMessage);
				dropsList.Visible = false;
				getDropButton.Visible = false;
				createDropButton.Visible = false;
				multiView1.ActiveViewIndex = 0;
				return;
			}
			
			// if we actually have drops returned
			if (drops.Count > 0)
			{
				// make the dropdown list and "get drop" buttons visible, since we know we have drops to look at
				dropsList.Visible = true;
				getDropButton.Visible = true;

				// keep track of the number of drops to see if we need to call the next page
				while (moreDrops == true)
				{
					moreDrops = false;

					// keep track of the number of drops. If we reach 30 we want to check the next page of results
					int count = 0;
					foreach (Drop drop in drops)
					{
						count++;
						// add the drop to the dropDown list
						dropsList.Items.Add (new ListItem (drop.Name + " (" + drop.AssetCount + ")", drop.Name));
					}
					if (count == 30)
{
						// we reach the max number of results for one page, see if the next page has more
						TimeKeeper.TimeIn ();
						drops = Drop.FindManagerDrops (resultsPage++);
						TimeKeeper.TimeOut ( this.Page, "Drop.FindManagerDrops page " + (resultsPage - 1).ToString () + ": ");
						if (drops.Count > 0)
						{
							// more drops, come and get them!
							moreDrops = true;
						}
					}
				}
			}
			
			// get drop info for the default selected item in the list
			getDropButton_Click(this, null);
		}
		
		// show the information for a single drop selected from the DropDownList
		public virtual void getDropButton_Click (object sender, EventArgs e)
		{
			// set the view to the drop view
			multiView1.ActiveViewIndex = 1;
			
			// hide these controls until we know if we should show them
			viewAssetsButton.Visible = false;
			assetsPage.Visible = false;
			assetsOrder.Visible = false;
			
			// clear the assetsPage dropDownList
			assetsPage.Items.Clear ();
			
			try
			{
				// get the drop information
				TimeKeeper.TimeIn ();
				GlobalDropRef.drop = Drop.Find (dropsList.SelectedValue.ToString ());
				TimeKeeper.TimeOut( this.Page, "Drop.Find: ");
			}
			catch ( ServiceException exc )
			{
				Response.Write( exc.serviceError + ": " + exc.serviceMessage );
			}
			
			// fill out the table with the drop information
			dropName.Text = GlobalDropRef.drop.Name;
			dropEmail.Text = GlobalDropRef.drop.Email;
			dropCurrentSize.Text = GlobalDropRef.drop.CurrentBytes.ToString ();
			dropMaxSize.Text = GlobalDropRef.drop.MaxBytes.ToString ();
			dropDescription.Text = GlobalDropRef.drop.Description;
			dropChatPassword.Text = GlobalDropRef.drop.ChatPassword;
			dropNumberOfAssets.Text = GlobalDropRef.drop.AssetCount.ToString ();
			
			// if we have more than 30 assets, show the page number control. This allows us to choose what page of
			// results we want returned (30 per page is default)
			if (GlobalDropRef.drop.AssetCount > 30)
			{
				// (number of assets / 30) => round up to next integer == number of pages
				int pages = (int)Math.Ceiling (((double)GlobalDropRef.drop.AssetCount / 30));
				// add page to dropDown and make it visible
				for (int x = 1; x <= pages; x++)
				{
					assetsPage.Items.Add (new ListItem ("page " + x.ToString (), x.ToString ()));
				}
				assetsPage.Visible = true;
			}
			
			// if we have assets, show the controls to view the assets
			if (GlobalDropRef.drop.AssetCount > 0)
			{
				viewAssetsButton.Visible = true;
				assetsOrder.Visible = true;
			}
			
			//add confirm dialogs for empty and destroy. If the javascript provided for the "onclick" attribute returns
			// false then the signal does not get propagated
			emptyDropButton.Attributes.Add ("onclick", "if ( !confirm('Are you sure you want to EMPTY all assets from the drop " + GlobalDropRef.drop.Name + "? All assets will be gone forever')){return false;}");
			destroyDropButton.Attributes.Add ("onclick", "if ( !confirm('Are you sure you want to DESTROY the drop " + GlobalDropRef.drop.Name + "? All assets will be gone forever')){return false;}");

		}
		
		// view the assets for a single drop
		public virtual void viewAssetsButton_Click (object sender, EventArgs e)
		{
			// put the value of the selected asset results page into the global class so that DropAssetLogic can access it
			GlobalDropRef.assetsPageSelectedValue = String.IsNullOrEmpty (assetsPage.SelectedValue) ? 1 : int.Parse(assetsPage.SelectedValue);

			// put asset order into global
			Order assetOrder = new Order ();
			assetOrder = (assetsOrder.SelectedValue == "oldest") ? Order.Oldest : Order.Newest;
			GlobalDropRef.assetOrder = assetOrder;
			
			GlobalDropRef.page = this.Page;
			
			// force the gridView to update and show the gridView page
			assetGridView.DataBind ();
			multiView1.ActiveViewIndex = 7;
		}
		
		// add an asset to a drop
		public virtual void addAssetButton_Click (object sender, EventArgs e)
		{
			Hashtable uploadifyOptions = new Hashtable();
			uploadifyOptions.Add( "auto", "false" ); 
			//uploadifyOptions.Add( "script", "'http://use.this.one.com/'");
			
			// add the script that uploadify needs to the page
			Type cstype = this.GetType ();
			string csname = "uploadifyScript";

			ClientScriptManager cs = Page.ClientScript; 
			
			if (!cs.IsStartupScriptRegistered (cstype, csname))
			{
				cs.RegisterStartupScript( cstype, csname, GlobalDropRef.drop.GetUploadifyForm( uploadifyOptions ), false );
			}
		
			dropNameLabel.Text = GlobalDropRef.drop.Name.ToString ();

			multiView1.ActiveViewIndex = 4;

		}
		
		public virtual void fileUploadButton_Click (object sender, EventArgs e)
		{

		}

		protected void createDropButton_Click (object sender, System.EventArgs e)
		{
			multiView1.ActiveViewIndex = 5;

		}
		
		protected void createDropGo_Click (object sender, EventArgs e)
		{
			try
			{
				Drop drop = Drop.Create (createDropName.Text, createDropDescription.Text, createDropEmailKey.Text, string.IsNullOrEmpty(createDropMaxSize.Text)?0:int.Parse(createDropMaxSize.Text), createDropChatPassword.Text);
				GlobalDropRef.drop = drop;
				
				// add this drop to the drop down list
				dropsList.Items.Add( new ListItem( GlobalDropRef.drop.Name + " (0)", GlobalDropRef.drop.Name ));
				dropsList.SelectedValue = GlobalDropRef.drop.Name;
				getDropButton_Click(this, null);
				
			}
			catch (ServiceException exc)
			{
				Console.WriteLine( exc.serviceMessage + ":" + exc.serviceError );
				StringBuilder sb = new StringBuilder( "<script>alert('");
				sb.Append( exc.serviceError ).Append( ": ");
				sb.Append( exc.serviceMessage);
				sb.Append( "')</script>");
				//Response.Write ("<script>alert('" + exc.serviceError + ":" + exc.serviceMessage + "')</script>");
				Response.Write ( sb.ToString());
			}
			catch
			{
				Response.Write( "<script>alert('unhandled exception')</script>");
			}
		
		}

		protected void deleteAssetButton_Click1 (object sender, System.EventArgs e)
		{
		}
		
		protected void emptyDropButton_Click (object sender, System.EventArgs e)
		{
			try
			{
				bool success = GlobalDropRef.drop.Empty ();
				if (success == true)
				{
					// update the GlobalDropRef
					GlobalDropRef.drop = Drop.Find (GlobalDropRef.drop.Name);
					
					// "press" the "Get Drop Info" button to reload the view to reflect the changes
					getDropButton_Click(this, null);
				}
				else
				{
					Response.Write ("<script>alert('there was a problem emptying the drop')</script>");
				}
				
			}
			catch
			{
				Response.Write ("<script>alert('exception thrown')</script>");
			}
		}
		
		protected void destroyDropButton_Click (object sender, System.EventArgs e)
		{
			try
			{
				bool success = GlobalDropRef.drop.Destroy ();
				if (success == true)
				{	
					// save current dropsList index. If this is the last item in the list we have to have decrease it by
					// 1
					int currIndex = (dropsList.SelectedIndex + 1 == dropsList.Items.Count)? dropsList.SelectedIndex - 1: dropsList.SelectedIndex;
					
					// remove this drop from the drop down list
					dropsList.Items.Remove (dropsList.Items.FindByValue (GlobalDropRef.drop.Name));
					
					// put the selection back to the same index
					dropsList.SelectedIndex = currIndex;
					
					//GlobalDropRef no longer refers to a valid drop
					GlobalDropRef.drop = null;
					
					// change to the empty view
					multiView1.ActiveViewIndex = 0;
				}
				else
				{
					Response.Write ("<script>alert('there was a problem destroying the drop')</script>");
				}
			}
			catch
			{
				Response.Write ("<script>alert('exception thrown')</script>");
			}
		}
		

		
	}
}

// class to bind the Asset object to the gridView table
public static class DropAssetsLogic
{
	// get assets so we can fill out the gridView
	public static ICollection GetAllAssets ()
	{
		// get the list of assets for the drop
		TimeKeeper.TimeIn();
		List<Asset> assets = GlobalDropRef.drop.Assets ( GlobalDropRef.assetsPageSelectedValue , GlobalDropRef.assetOrder );
		TimeKeeper.TimeOut( GlobalDropRef.page, "Drop.Assets: ");

		return assets;
	}
}