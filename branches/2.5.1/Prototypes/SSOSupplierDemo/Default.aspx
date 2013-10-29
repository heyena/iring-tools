<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SSOSupplierDemo.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
<link rel="stylesheet" href="images/HigherGround.css" type="text/css" />
<title>Supplier Portal SSO Demo</title></head>
</head>
<body>

<div id="wrap">

	<div id="top-bg"></div>
				
	<!--header -->
	<div id="header">			
				
		<h1 id="logo-text"><a href="#" title="">supplier<span>portal</span></a></h1>		
		<p id="slogan">sso demonstration application</p>		
			
		<div id="header-links">
		<p>
			<a href="#">Home</a> | 
			<a href="#">Contact</a> | 
			<a href="#">Site Map</a>			
		</p>		
		</div>		
	<!--header ends-->					
	</div>
		
	<div id="header-photo"></div>		
		
			
	<!-- content-wrap starts -->
	<div id="content-wrap">
	
		<div id="main">
		    <form id="form1" runat="server">
    <h3><asp:Label ID="lblInfo" runat="server" Text=""></asp:Label></h3>
    <asp:ListBox ID="lbInfo"
            runat="server" Width="400px"></asp:ListBox>
            
            <div id="oauthwrapper" style="width: 200px">
    OAuth Header:<div id="oauthheader"><asp:Literal ID="litOAuthHeader" runat="server"></asp:Literal></div></div>
        </form>
        
        This is the home page.
    </div>
    
		<div id="sidebar">
			
			<h3>Search Box</h3>	
			<form action="#" class="searchform">
				<p>
				<input name="search_query" class="textbox" type="text" />
  				<input name="search" class="button" value="Search" type="submit" />
				</p>			
			</form>	
					
			<h3>Sidebar Menu</h3>
			<ul class="sidemenu">				
				<li><a href="<%= ConfigurationManager.AppSettings["SupplierWebSiteAddress"] %>">Supplier Portal</a></li>
				<li><a href="<%= ConfigurationManager.AppSettings["BUWebSiteAddress"] %>">Bechtel University</a></li>
				<li><a href="<%= ConfigurationManager.AppSettings["EMailWebSiteAddress"] %>">EMail Client</a></li>
				<li><a href="http://www.styleshout.com/">More Free Templates</a></li>	
				<li><a href="http://www.4templates.com/?aff=ealigam">Premium Templates</a></li>	
			</ul>	
				
			<h3>Links</h3>
			<ul class="sidemenu">
				<li><a href="http://www.pdphoto.org/">PDPhoto.org</a></li>
				<li><a href="http://www.squidfingers.com/patterns/">Squidfingers | Patterns</a></li>
				<li><a href="http://www.alistapart.com">Alistapart</a></li>					
				<li><a href="http://www.cssremix.com">CSS Remix</a></li>
				<li><a href="http://www.cssmania.com">CSS Mania</a></li>				
			</ul>
			
			<h3>Sponsors</h3>
			<ul class="sidemenu">
				<li><a href="http://www.4templates.com/?aff=ealigam"><strong>4templates</strong></a> <br /> Low Cost Hi-Quality Templates</li>
				<li><a href="http://store.templatemonster.com?aff=ealigam"><strong>TemplateMonster</strong></a> <br /> Delivering the Best Templates on the Net!</li>
				<li><a href="http://tinyurl.com/3cgv2m"><strong>Text Link Ads</strong></a> <br /> Monetized your website</li>
				<li><a href="http://www.fotolia.com/partner/114283"><strong>Fotolia</strong></a> <br /> Free stock images or from $1</li>
				<li><a href="http://www.dreamstime.com/res338619"><strong>Dreamstime</strong></a> <br /> Lowest Price for High Quality Stock Photos</li>
				<li><a href="http://www.dreamhost.com/r.cgi?287326"><strong>Dreamhost</strong></a> <br /> Premium webhosting</li>
			</ul>
				
			<h3>Wise Words</h3>
			<p>&quot;Great works are performed not by strength,
			but by perseverance.&quot; </p>
					
			<p class="align-right">- Samuel Johnson</p>
					
			<h3>Support Styleshout</h3>
			<p>If you are interested in supporting my work and would like to contribute, you are
			welcome to make a small donation through the 
			<a href="http://www.styleshout.com/">donate link</a> on my website - it will 
			be a great help and will surely be appreciated.</p>			
						
		<!-- sidebar ends -->		
		</div>		
		
		
	<!-- content-wrap ends-->	
	</div>
		
	<!-- footer starts -->		
	<div id="footer-wrap">
		<div id="footer-columns">
	
			<div class="col3">
				<h3>Tincidunt</h3>
				<ul>
					<li><a href="#">consequat molestie</a></li>
					<li><a href="#">sem justo</a></li>
					<li><a href="#">semper</a></li>
					<li><a href="#">magna sed purus</a></li>
					<li><a href="#">tincidunt</a></li>
				</ul>
			</div>

			<div class="col3-center">
				<h3>Sed purus</h3>
				<ul>
					<li><a href="#">consequat molestie</a></li>
					<li><a href="#">sem justo</a></li>
					<li><a href="#">semper</a></li>
					<li><a href="#">magna sed purus</a></li>
					<li><a href="#">tincidunt</a></li>
				</ul>
			</div>

			<div class="col3">
				<h3>Praesent</h3>
				<ul>
					<li><a href="#">consequat molestie</a></li>
					<li><a href="#">sem justo</a></li>
					<li><a href="#">semper</a></li>
					<li><a href="#">magna sed purus</a></li>
					<li><a href="#">tincidunt</a></li>					
				</ul>
			</div>
		<!-- footer-columns ends -->
		</div>	
	
		<div id="footer-bottom">		
			
			<p>
			&copy; 2006 <strong>Your Company</strong> | 
			Design by: <a href="http://www.styleshout.com/">styleshout</a> | 
			Valid <a href="http://validator.w3.org/check?uri=referer">XHTML</a> | 
			<a href="http://jigsaw.w3.org/css-validator/check/referer">CSS</a>
			
   		&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
			
			<a href="#">Home</a>&nbsp;|&nbsp;
   		<a href="#">Sitemap</a>&nbsp;|&nbsp;
	   	<a href="#">RSS Feed</a>
   		</p>		
			
		</div>	

<!-- footer ends-->
</div>
<!-- wrap ends here -->
</div>    

</body>
</html>
