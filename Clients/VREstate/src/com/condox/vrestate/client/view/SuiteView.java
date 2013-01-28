package com.condox.vrestate.client.view;

import com.condox.vrestate.client.Log;
import com.condox.vrestate.client.Options;
import com.condox.vrestate.client.document.SuiteType;
import com.condox.vrestate.client.ge.GE;
import com.condox.vrestate.client.interactor.SuiteInteractor;
import com.condox.vrestate.client.view.Camera.Camera;
import com.condox.vrestate.client.view.GeoItems.IGeoItem;
import com.condox.vrestate.client.view.GeoItems.SuiteGeoItem;
import com.google.gwt.dom.client.Element;
import com.google.gwt.http.client.Request;
import com.google.gwt.http.client.RequestBuilder;
import com.google.gwt.http.client.RequestCallback;
import com.google.gwt.http.client.RequestException;
import com.google.gwt.http.client.Response;
import com.google.gwt.http.client.URL;
import com.google.gwt.json.client.JSONNumber;
import com.google.gwt.json.client.JSONObject;
import com.google.gwt.json.client.JSONString;
import com.nitrous.gwt.earth.client.api.GEHtmlDivBalloon;
import com.nitrous.gwt.earth.client.api.GEVisibility;
import com.nitrous.gwt.earth.client.api.KmlFeature;
import com.nitrous.gwt.earth.client.api.KmlObject;
import com.nitrous.gwt.earth.client.api.event.KmlLoadCallback;
import com.google.gwt.user.client.Window;

public class SuiteView extends _GEView implements RequestCallback {

	private static boolean isMoreInfoVisible = false;

	SuiteGeoItem suiteGeo = null;
	
	public SuiteView(IGeoItem geoItem) {
		super(geoItem);
		
		suiteGeo = (SuiteGeoItem) geoItem; 
	}

	@Override
	public void setEnabled(boolean enabling) {
		super.setEnabled(enabling);
		if (enabling) {
			GE.getPlugin().getNavigationControl().setVisibility(GEVisibility.VISIBILITY_HIDE);
			if (_interactor == null)
				_interactor = new SuiteInteractor(this);
			_interactor.setEnabled(true);
		} else {
			_interactor.setEnabled(false);
			_interactor = null;
			HideMoreInfo();
		}
	}

	@Override
	public void Select(String type, int id) {
		if (type == null)
		{
			_AbstractView.Pop();
			return;
		}

		if (type.equals("suite") && id == theGeoItem.getId())
			return; // do nothing when they clicked the suite that already opened

		if (type.equals("suite"))
		{
			IGeoItem suiteGeo = _AbstractView.getSiteGeoItem(id);
			SuiteView suiteView = new SuiteView(suiteGeo);
			_AbstractView.Pop_Push(suiteView);
		}
	}

	// @Override
	// protected void onPlacemarkClick(String snippet) {
	// JSONObject obj = JSONParser.parseLenient(snippet).isObject();
	// String type = obj.get("type").isString().stringValue();
	// int id = Integer.valueOf((int) obj.get("id").isNumber().doubleValue());
	// if (type.equals("BUILDING")) {
	// Building building = Building.get(id);
	// if (Viewable.isSelected(building))
	// Site.get(building.getParent_id()).Select();
	// else
	// BuildingView.Create(id);
	// }
	// else if (type.equals("SUITE")) {
	// Suite suite = Suite.get(id);
	// SuiteView.Create(id);
	// // if (Viewable.isSelected(suite))
	// // Site.get(suite.getParent_id()).Select();
	// // else
	// // BuildingView.Create(id);
	// }
	//
	// super.onPlacemarkClick(snippet);
	// }

	public void ShowMoreInfo() {
		if (isMoreInfoVisible)
			return;
		isMoreInfoVisible = true;
		GEHtmlDivBalloon balloon = GE.getPlugin().createHtmlDivBalloon("");
		balloon.setCloseButtonEnabled(false);
		balloon.setContentDiv(Options.SUITE_INFO_TEMPLATE);
		KmlFeature feature = (KmlFeature) suiteGeo.getExtendedDataLabel();
		Log.write("KML: " + feature.getKml());
		balloon.setFeature(feature);
		GE.getPlugin().setBalloon(balloon);
		addElement(((Element) balloon.getContentDiv()), getJsonParams());
	};

	public void HideMoreInfo() {
		if (!isMoreInfoVisible)
			return;
		isMoreInfoVisible = false;
		GEHtmlDivBalloon balloon = (GEHtmlDivBalloon) GE.getPlugin()
				.getBalloon();
		removeElement(((Element) balloon.getContentDiv()));
		GE.getPlugin().setBalloon(null);
	}

	private String getJsonParams() {
		JSONObject obj = new JSONObject();
		if (suiteGeo.suite.getPrice() > 0)
			obj.put("price", new JSONString("$" + suiteGeo.suite.getPrice()));
		obj.put("suitName", new JSONString(suiteGeo.suite.getName()));
		obj.put("Floor", new JSONString(suiteGeo.suite.getFloor_name()));

		SuiteType type = suiteGeo.suite.getSuiteType();

		obj.put("bedrooms", new JSONString(type.getRoomsStr()));
		if (type.getBalconies() > 0)
			obj.put("balcony", new JSONNumber(type.getBalconies()));
		else
			obj.put("balcony", new JSONString("none"));
		//obj.put("ceiling", new JSONNumber(suiteGeo.suite.getCeiling_height_ft()));

		String vTourUrl = suiteGeo.suite.getVTourUrl(); 
		if (vTourUrl != null && vTourUrl.length() > 0)
			obj.put("virtualTour", new JSONString(vTourUrl));
			 
		String floorPlanUrl = type.getFloorPlanUrl(); 
		if (floorPlanUrl != null && floorPlanUrl.length() > 0)
			obj.put("more", new JSONString(floorPlanUrl));
			 
		obj.put("panoramicViewURL", new JSONString(""));
		
		if (type.getArea() > 0)
			obj.put("area", new JSONNumber(type.getArea()));

		String infoUrl = suiteGeo.suite.getInfoUrl();
		if (infoUrl != null && infoUrl.length() > 0)
			obj.put("moreInfo", new JSONString(infoUrl));
		else if (Options.DEBUG_MODE)
			obj.put("moreInfo", new JSONString("http://www.google.com"));
		
		// obj.put("mail", new JSONString("MailUrl"));
		// obj.put("phone", new JSONString("123456789"));
		// Log.write("json:" + obj.toString());
		// Log.write("balconies: " + suite_type.balconies);
		// Log.write("suite_type: " + suite_type.name);
		Log.write(obj.toString());
		return obj.toString();
	};

	public native void addElement(Element element, String json) /*-{
		//********************************
		$doc.getElementsByTagName('body')[0].appendChild(element);
		element.style.left = "10px";
		var scripts = element.getElementsByTagName("script");

		for (i = 0; i < scripts.length; i++) {
			// if src, eval it, otherwise eval the body
			if (scripts[i].hasAttribute("src")) {
				var src = scripts[i].getAttribute("src");
				var script = $doc.createElement('script');
				script.setAttribute("src", src);
				$doc.getElementsByTagName('body')[0].appendChild(script);
			} else {
				$wnd.eval(scripts[i].innerHTML);
			}
		}
		//********************************
		var suite = this;
		var show_panoramic_view = function() {
			suite.@com.condox.vrestate.client.view.SuiteView::ShowPanoramicView()();
		}
		var show_more = function() {
			suite.@com.condox.vrestate.client.view.SuiteView::ShowMore()();
			//		return false;
		}
//		$wnd.alert(element.innerHTML);
		$wnd.project(element, json, show_panoramic_view, show_more);
//		$wnd.alert(element.innerHTML);
	}-*/;

	public native void removeElement(Element element) /*-{
		element.parentNode.removeChild(element);
	}-*/;

	private void ShowPanoramicView() {
		IGeoItem suiteGeo = _AbstractView.getSuiteGeoItem(theGeoItem.getId());
		_AbstractView.Push(new PanoramicView(suiteGeo));
	}

	String floorPlanHTML = null;
	private void ShowMore() throws RequestException {
		SuiteType type = suiteGeo.suite.getSuiteType();
		String floorPlanUrl = type.getFloorPlanUrl(); 
		if (floorPlanUrl != null) {
			if (!floorPlanUrl.endsWith(".html") &&
				!floorPlanUrl.endsWith(".htm"))
				Window.open(floorPlanUrl, "_blank", null);
			else
			{
				RequestBuilder requestBldr = new RequestBuilder(RequestBuilder.GET, URL.encode(floorPlanUrl));
				requestBldr.setCallback(this);
				requestBldr.setHeader("Access-Control-Allow-Origin","http://myserver");
				requestBldr.send();
				floorPlanHTML = floorPlanUrl;
			}
		}
	}
	
	@Override
	public void onResponseReceived(Request request, Response response) {
		String html = URL.decodeQueryString(response.getText());
		//if (html.endsWith("</html>"))
		{
			html = html.replace("images/", floorPlanHTML.substring(0, floorPlanHTML.lastIndexOf("/") + 1) + "images/");
			Log.write(floorPlanHTML);
			Log.write(html);

			html = html.replace("_parameter_SuiteNo",suiteGeo.getName());
			html = html.replace("_parameter_FloorNo",suiteGeo.getFloor_name());
			html = html.replace("_parameter_CellingHeight",String.valueOf(suiteGeo.getCellingHeight()) + " ft.");
			html = html.replace("_parameter_Price",String.valueOf(suiteGeo.getPrice()));
			open(html);
		}
	}

	private native void open(String html) /*-{
		var wnd = window.open("","_blank","");
		wnd.document.write(html);
	}-*/;	
	
	public void Update(double speed) {
	}

	@Override
	public void onViewChanged() {
	}

	@Override
	public void setupCamera() {
		setupStandardLookAtCamera();
        _camera.attributes.Heading_d = Camera.NormalizeHeading_d(theGeoItem.getPosition().getHeading() + 180);
	}

	@Override
	public void onTransitionStopped() {
		onHeadingChanged();
		ShowMoreInfo();
	}

	@Override
	public void onError(Request request, Throwable exception) {
		// TODO Auto-generated method stub
		
	}

}
