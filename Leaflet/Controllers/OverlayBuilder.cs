﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ThinkGeo.MapSuite.Drawing;
using ThinkGeo.MapSuite.Layers;
using ThinkGeo.MapSuite.Styles;
using ThinkGeo.MapSuite.WebApi;

namespace Labeling
{
    public static class OverlayBuilder
    {
        // Initialize the grid size for filtering the labels.
        private static Dictionary<string, int> gridSizeConfigurations = new Dictionary<string, int>() 
        { 
            { "Small", 100 },
            { "Medium", 500 },
            { "Large", 1000 } 
        };

        /// <summary>
        /// Gets an overlay used as base map.
        /// </summary>
        public static LayerOverlay GetOverlayAsBaseMap()
        {
            // Create the LayerOverlay for displaying the map.
            LayerOverlay labelingStyleOverlay = new LayerOverlay();

            // Create the parcels layer.
            ShapeFileFeatureLayer parcelLayer = new ShapeFileFeatureLayer(string.Format(@"{0}\{1}", GetBaseDirectory(), "Parcels.shp"));
            parcelLayer.ZoomLevelSet.ZoomLevel10.DefaultAreaStyle = new AreaStyle(new GeoPen(GeoColor.FromHtml("#d6d5d4"), 2), new GeoSolidBrush(GeoColor.FromHtml("#faf7f3")), PenBrushDrawingOrder.PenFirst);
            parcelLayer.ZoomLevelSet.ZoomLevel10.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;
            labelingStyleOverlay.Layers.Add("parcel", parcelLayer);

            // Create the pois layer.
            ShapeFileFeatureLayer restaurantsLayer = new ShapeFileFeatureLayer(string.Format(@"{0}\{1}", GetBaseDirectory(), "Pois.shp"));
            restaurantsLayer.ZoomLevelSet.ZoomLevel10.DefaultPointStyle = new PointStyle(PointSymbolType.Circle, new GeoSolidBrush(GeoColor.FromHtml("#99cc33")), new GeoPen(GeoColor.FromHtml("#666666"), 1), 3);
            restaurantsLayer.ZoomLevelSet.ZoomLevel10.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;
            // Set DrawingMarginPercentage to a proper value to avoid some labels are cut-off
            restaurantsLayer.DrawingMarginPercentage = 300;
            labelingStyleOverlay.Layers.Add("street", restaurantsLayer);

            // Create the streets layer.
            ShapeFileFeatureLayer streetLayer = new ShapeFileFeatureLayer(string.Format(@"{0}\{1}", GetBaseDirectory(), "Street.shp"));

            // Add a copy of road stype to make sure the end of line shape is round cap.
            ClassBreakStyle roadStyleCopy = new ClassBreakStyle("Type");
            ClassBreak localRoadBreak = new ClassBreak();
            localRoadBreak.Value = 6.9;
            localRoadBreak.DefaultLineStyle = new LineStyle(new GeoPen(GeoColor.FromHtml("#d6d5d4"), 8f), new GeoPen(GeoColor.FromHtml("#ffffff"), 6f));
            localRoadBreak.DefaultLineStyle.CenterPen.EndCap = DrawingLineCap.Round;
            localRoadBreak.DefaultLineStyle.OuterPen.EndCap = DrawingLineCap.Round;
            roadStyleCopy.ClassBreaks.Add(localRoadBreak);
            streetLayer.ZoomLevelSet.ZoomLevel10.CustomStyles.Add(roadStyleCopy);

            // Define the style of streets based on its type.
            ClassBreakStyle roadStyle = new ClassBreakStyle("Type");
            roadStyle.BreakValueInclusion = BreakValueInclusion.ExcludeValue;

            ClassBreak highwayBreak = new ClassBreak();
            highwayBreak.Value = 0.9;
            highwayBreak.DefaultLineStyle = new LineStyle(new GeoPen(GeoColor.FromHtml("#a2a09c"), 12f), new GeoPen(GeoColor.FromHtml("#f7c67f"), 10f));
            highwayBreak.DefaultLineStyle.CenterPen.EndCap = DrawingLineCap.Round;
            highwayBreak.DefaultLineStyle.OuterPen.EndCap = DrawingLineCap.Round;
            roadStyle.ClassBreaks.Add(highwayBreak);

            ClassBreak majorRoadBreak = new ClassBreak();
            majorRoadBreak.Value = 3.8;
            majorRoadBreak.DefaultLineStyle = new LineStyle(new GeoPen(GeoColor.FromHtml("#d6d5d4"), 10f), new GeoPen(GeoColor.FromHtml("#f5e8cb"), 8f));
            majorRoadBreak.DefaultLineStyle.CenterPen.EndCap = DrawingLineCap.Round;
            majorRoadBreak.DefaultLineStyle.OuterPen.EndCap = DrawingLineCap.Round;
            roadStyle.ClassBreaks.Add(majorRoadBreak);

            ClassBreak minorRoadBreak = new ClassBreak();
            minorRoadBreak.Value = 6.9;
            minorRoadBreak.DefaultLineStyle = new LineStyle(new GeoPen(GeoColor.SimpleColors.Transparent), new GeoPen(GeoColor.SimpleColors.Transparent));
            roadStyle.ClassBreaks.Add(minorRoadBreak);
            streetLayer.ZoomLevelSet.ZoomLevel10.CustomStyles.Add(roadStyle);

            streetLayer.ZoomLevelSet.ZoomLevel10.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;
            streetLayer.DrawingMarginPercentage = 200;
            labelingStyleOverlay.Layers.Add("poi", streetLayer);

            return labelingStyleOverlay;
        }

        /// <summary>
        /// Gets an overlay applied with customized labeling style.
        /// </summary>
        public static LayerOverlay GetOverlayWithCustomLabeling(string overlayId, string accessId)
        {
            LayerOverlay customLabelingOverlay = new LayerOverlay();

            ShapeFileFeatureLayer poiLayer = new ShapeFileFeatureLayer(string.Format(@"{0}\{1}", GetBaseDirectory(), "Pois.shp"));
            customLabelingOverlay.Layers.Add("customLabeling_Label", poiLayer);

            poiLayer.ZoomLevelSet.ZoomLevel10.DefaultPointStyle = new PointStyle(PointSymbolType.Circle, new GeoSolidBrush(GeoColor.FromHtml("#99cc33")), new GeoPen(GeoColor.FromHtml("#666666"), 1), 7);
            poiLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle = new CustomTextStyle("Name", new GeoFont("Arail", 9, DrawingFontStyles.Bold), new GeoSolidBrush(GeoColor.SimpleColors.Black));
            poiLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.HaloPen = new GeoPen(GeoColor.StandardColors.White, 1);
            poiLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.Mask = new AreaStyle();
            poiLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.XOffsetInPixel = 10;
            poiLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.YOffsetInPixel = 10;
            poiLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.PointPlacement = PointPlacement.Center;
            poiLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.OverlappingRule = LabelOverlappingRule.NoOverlapping;
            poiLayer.ZoomLevelSet.ZoomLevel10.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;
            // Set DrawingMarginPercentage to a proper value to avoid some labels are cut-off
            poiLayer.DrawingMarginPercentage = 600;

            Dictionary<string, string> savedStyle = GetSavedLabelStyleByAccessId(overlayId, accessId);
            if (savedStyle != null)
            {
                UpdateCustomLabelingStyle(customLabelingOverlay, savedStyle);
            }

            return customLabelingOverlay;
        }

        /// <summary>
        /// Gets the labeling style applied to a specified overlay with customized labeling style.
        /// </summary>
        public static Dictionary<string, object> GetCustomLabelingStyle(LayerOverlay overlay)
        {
            Dictionary<string, object> styles = new Dictionary<string, object>();

            ShapeFileFeatureLayer layer = overlay.Layers[0] as ShapeFileFeatureLayer;
            CustomTextStyle customLabelStyle = layer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle as CustomTextStyle;
            if (customLabelStyle != null)
            {
                styles.Add("minSize", customLabelStyle.MinFontSize.ToString());
                styles.Add("maxSize", customLabelStyle.MaxFontSize.ToString());
            }

            return styles;
        }

        /// <summary>
        /// Updates the specified style to a specified overlay.
        /// </summary>
        private static void UpdateCustomLabelingStyle(LayerOverlay overlay, Dictionary<string, string> styles)
        {
            float minSize = 0, maxSize = 0;

            if (float.TryParse(styles["minSize"], out minSize) && float.TryParse(styles["maxSize"], out maxSize))
            {
                foreach (ShapeFileFeatureLayer item in overlay.Layers)
                {
                    CustomTextStyle customLabelStyle = item.ZoomLevelSet.ZoomLevel10.DefaultTextStyle as CustomTextStyle;
                    customLabelStyle.MinFontSize = minSize;
                    customLabelStyle.MaxFontSize = maxSize;
                }
            }
        }

        /// <summary>
        /// Gets an overlay for showing streets.
        /// </summary>
        public static LayerOverlay GetOverlayWithLabelingLine(string overlayId, string accessId)
        {
            LayerOverlay labelingLinesOverlay = new LayerOverlay();

            ShapeFileFeatureLayer streetLayer = new ShapeFileFeatureLayer(string.Format(@"{0}\{1}", GetBaseDirectory(), "Street.shp"));
            labelingLinesOverlay.Layers.Add("street_Label", streetLayer);

            // Create a classBreakStyle for different type of roads
            ClassBreakStyle roadStyle = new ClassBreakStyle("Type");
            roadStyle.BreakValueInclusion = BreakValueInclusion.ExcludeValue;

            ClassBreak break1 = new ClassBreak();
            break1.Value = 0.9;
            break1.DefaultTextStyle = new TextStyle("ROAD_NAME", new GeoFont("Arial", 12, DrawingFontStyles.Regular), new GeoSolidBrush(GeoColor.FromHtml("#666666")));
            break1.DefaultTextStyle.HaloPen = new GeoPen(GeoColor.SimpleColors.White, 2);
            break1.DefaultTextStyle.SplineType = SplineType.ForceSplining;
            break1.DefaultTextStyle.Mask = new AreaStyle();
            roadStyle.ClassBreaks.Add(break1);

            ClassBreak break2 = new ClassBreak();
            break2.Value = 3.8;
            break2.DefaultTextStyle = new TextStyle("ROAD_NAME", new GeoFont("Arial", 8, DrawingFontStyles.Bold), new GeoSolidBrush(GeoColor.FromHtml("#666666")));
            break2.DefaultTextStyle.HaloPen = new GeoPen(GeoColor.SimpleColors.White, 1);
            break2.DefaultTextStyle.SplineType = SplineType.ForceSplining;
            break2.DefaultTextStyle.Mask = new AreaStyle();
            roadStyle.ClassBreaks.Add(break2);

            ClassBreak break3 = new ClassBreak();
            break3.Value = 7;
            break3.DefaultTextStyle = new TextStyle("ROAD_NAME", new GeoFont("Arial", 6, DrawingFontStyles.Regular), new GeoSolidBrush(GeoColor.FromHtml("#666666")));
            break3.DefaultTextStyle.Mask = new AreaStyle();
            break3.DefaultTextStyle.SplineType = SplineType.ForceSplining;
            roadStyle.ClassBreaks.Add(break3);

            streetLayer.ZoomLevelSet.ZoomLevel10.CustomStyles.Add(roadStyle);
            streetLayer.ZoomLevelSet.ZoomLevel10.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;
            // Set DrawingMarginPercentage to a proper value to avoid some labels are cut-off
            streetLayer.DrawingMarginPercentage = 200;

            Dictionary<string, string> savedStyle = GetSavedLabelStyleByAccessId(overlayId, accessId);
            if (savedStyle != null)
            {
                UpdateLabelingLineStyle(labelingLinesOverlay, savedStyle);
            }

            return labelingLinesOverlay;
        }

        /// <summary>
        /// Gets the labeling style applied to streets.
        /// </summary>
        public static Dictionary<string, object> GetLabelingLineStyle(LayerOverlay overlay)
        {
            ShapeFileFeatureLayer layer = overlay.Layers[0] as ShapeFileFeatureLayer;
            ClassBreakStyle classBreakStyle = layer.ZoomLevelSet.ZoomLevel10.CustomStyles[0] as ClassBreakStyle;

            Dictionary<string, object> styles = new Dictionary<string, object>();
            if (classBreakStyle != null && classBreakStyle.ClassBreaks.Count > 0)
            {
                styles.Add("spline", classBreakStyle.ClassBreaks[0].DefaultTextStyle.SplineType.ToString());
                styles.Add("lineSegmentRatio", classBreakStyle.ClassBreaks[0].DefaultTextStyle.TextLineSegmentRatio.ToString());
            }

            return styles;
        }

        /// <summary>
        /// Updates the specified style to a specified overlay.
        /// </summary>
        private static void UpdateLabelingLineStyle(LayerOverlay overlay, Dictionary<string, string> styles)
        {
            SplineType splineType = SplineType.Default;
            double lineSegmentRatio = 0;

            if (Enum.TryParse<SplineType>(styles["spline"], out splineType) && double.TryParse(styles["lineSegmentRatio"], out lineSegmentRatio))
            {
                foreach (string layerId in overlay.Layers.GetKeys())
                {
                    ShapeFileFeatureLayer featureLayer = overlay.Layers[layerId] as ShapeFileFeatureLayer;
                    ClassBreakStyle classBreakStyle = featureLayer.ZoomLevelSet.ZoomLevel10.CustomStyles[0] as ClassBreakStyle;
                    foreach (var classBreak in classBreakStyle.ClassBreaks)
                    {
                        classBreak.DefaultTextStyle.SplineType = splineType;
                        classBreak.DefaultTextStyle.TextLineSegmentRatio = lineSegmentRatio;
                    }
                }
            }
        }

        /// <summary>
        /// Gets an overlay for showing points.
        /// </summary>
        public static LayerOverlay GetOverlayWithLabelingPoint(string overlayId, string accessId)
        {
            LayerOverlay labelingPointsOverlay = new LayerOverlay();

            ShapeFileFeatureLayer poiLayer = new ShapeFileFeatureLayer(string.Format(CultureInfo.InvariantCulture, @"{0}\{1}", GetBaseDirectory(), "Pois.shp"));
            labelingPointsOverlay.Layers.Add("poi_Label", poiLayer);

            poiLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle = new TextStyle("Name", new GeoFont("Arail", 9, DrawingFontStyles.Bold), new GeoSolidBrush(GeoColor.SimpleColors.Black));
            poiLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.HaloPen = new GeoPen(GeoColor.StandardColors.White, 2);
            poiLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.Mask = new AreaStyle(new GeoPen(GeoColor.FromHtml("#999999"), 1), new GeoSolidBrush(new GeoColor(100, GeoColor.FromHtml("#cccc99"))), PenBrushDrawingOrder.PenFirst);
            poiLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.XOffsetInPixel = 0;
            poiLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.YOffsetInPixel = 8;
            poiLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.PointPlacement = PointPlacement.UpperCenter;
            poiLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.OverlappingRule = LabelOverlappingRule.NoOverlapping;
            poiLayer.ZoomLevelSet.ZoomLevel10.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;
            // Set DrawingMarginPercentage to a proper value to avoid some labels are cut-off
            poiLayer.DrawingMarginPercentage = 100;

            Dictionary<string, string> savedStyle = GetSavedLabelStyleByAccessId(overlayId, accessId);
            if (savedStyle != null)
            {
                UpdateLabelingPointStyle(labelingPointsOverlay, savedStyle);
            }


            return labelingPointsOverlay;
        }

        /// <summary>
        /// Gets the labeling style applied to streets.
        /// </summary>
        public static Dictionary<string, object> GetLabelingPointStyle(LayerOverlay overlay)
        {
            Dictionary<string, object> styles = new Dictionary<string, object>();

            ShapeFileFeatureLayer layer = overlay.Layers[0] as ShapeFileFeatureLayer;
            styles.Add("placement", layer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.PointPlacement.ToString());
            styles.Add("xoffset", layer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.XOffsetInPixel.ToString());
            styles.Add("yoffset", layer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.YOffsetInPixel.ToString());

            return styles;
        }

        /// <summary>
        /// Updates the specified style to a specified overlay.
        /// </summary>
        private static void UpdateLabelingPointStyle(LayerOverlay overlay, Dictionary<string, string> styles)
        {
            PointPlacement placement = PointPlacement.Center;
            float xOffset = 0f;
            float yOffset = 0f;
            if (Enum.TryParse<PointPlacement>(styles["placement"], out placement) && float.TryParse(styles["xoffset"], out xOffset) && float.TryParse(styles["yoffset"], out yOffset))
            {
                foreach (string layerId in overlay.Layers.GetKeys())
                {
                    ShapeFileFeatureLayer featureLayer = overlay.Layers[layerId] as ShapeFileFeatureLayer;
                    featureLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.PointPlacement = placement;
                    featureLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.XOffsetInPixel = xOffset;
                    featureLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.YOffsetInPixel = yOffset;
                }
            }
        }

        /// <summary>
        /// Gets an overlay for showing polygons(Parcel).
        /// </summary>
        public static LayerOverlay GetOverlayWithLabelingPolygon(string overlayId, string accessId)
        {
            LayerOverlay labelingPolygonsOverlay = new LayerOverlay();

            ShapeFileFeatureLayer subdivisionsLayer = new ShapeFileFeatureLayer(string.Format(@"{0}\{1}", GetBaseDirectory(), "Subdivisions.shp"));
            labelingPolygonsOverlay.Layers.Add("subdivision_Label", subdivisionsLayer);

            subdivisionsLayer.ZoomLevelSet.ZoomLevel10.DefaultAreaStyle = AreaStyles.CreateSimpleAreaStyle(GeoColor.StandardColors.White, GeoColor.FromHtml("#9C9C9C"), 1);
            subdivisionsLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle = new TextStyle("NAME_COMMO", new GeoFont("Arail", 9, DrawingFontStyles.Bold), new GeoSolidBrush(GeoColor.SimpleColors.Black));
            subdivisionsLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.HaloPen = new GeoPen(GeoColor.StandardColors.White, 1);
            subdivisionsLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.Mask = new AreaStyle();
            subdivisionsLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.BestPlacement = true;
            // No overlapping will make it looks better if a lot of labels are together.
            subdivisionsLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.OverlappingRule = LabelOverlappingRule.NoOverlapping;
            subdivisionsLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.DuplicateRule = LabelDuplicateRule.NoDuplicateLabels;
            subdivisionsLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.SuppressPartialLabels = true;
            subdivisionsLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.FittingPolygon = true;
            subdivisionsLayer.ZoomLevelSet.ZoomLevel10.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;

            Dictionary<string, string> savedStyle = GetSavedLabelStyleByAccessId(overlayId, accessId);
            if (savedStyle != null)
            {
                UpdateLabelingPolygonStyle(labelingPolygonsOverlay, savedStyle);
            }

            return labelingPolygonsOverlay;
        }

        /// <summary>
        /// Gets the labeling style applied to streets.
        /// </summary>
        public static Dictionary<string, object> GetLabelingPolygonStyle(LayerOverlay overlay)
        {
            Dictionary<string, object> styles = new Dictionary<string, object>();

            ShapeFileFeatureLayer layer = overlay.Layers[0] as ShapeFileFeatureLayer;
            styles.Add("fittingPolygon", layer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.FittingPolygon);
            styles.Add("labelAllPolygonParts", layer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.LabelAllPolygonParts);

            return styles;
        }

        /// <summary>
        /// Updates the specified style to a specified overlay.
        /// </summary>
        private static void UpdateLabelingPolygonStyle(LayerOverlay overlay, Dictionary<string, string> styles)
        {
            bool fittingFactors = true;
            bool labelAllPolygonParts = true;
            if (bool.TryParse(styles["fittingPolygon"], out fittingFactors) && bool.TryParse(styles["labelAllPolygonParts"], out labelAllPolygonParts))
            {
                foreach (string layerId in overlay.Layers.GetKeys())
                {
                    ShapeFileFeatureLayer featureLayer = overlay.Layers[layerId] as ShapeFileFeatureLayer;
                    featureLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.FittingPolygon = fittingFactors;
                    featureLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.LabelAllPolygonParts = labelAllPolygonParts;
                }
            }
        }

        /// <summary>
        /// Gets an overlay of displaying kinds of labels.
        /// </summary>
        public static LayerOverlay GetOverlayWithLabelingStyle(string overlayId, string accessId)
        {
            LayerOverlay labelingStyleOverlay = new LayerOverlay();

            /*Load Frisco Parcel Layer*/
            ShapeFileFeatureLayer parcelLayer = GetLabelingStyleParcelLayer();
            labelingStyleOverlay.Layers.Add("parcel_Label", parcelLayer);

            /*Load Frisco Street Layer*/
            ShapeFileFeatureLayer streetLabelingLayer = GetLabelingStreetLayer();
            labelingStyleOverlay.Layers.Add("street_Label", streetLabelingLayer);

            /*Load Frisco Pois Layer*/
            ShapeFileFeatureLayer poisLayer = GetLabelingStylePoisLayer();
            labelingStyleOverlay.Layers.Add("poi_Label", poisLayer);

            Dictionary<string, string> savedStyle = GetSavedLabelStyleByAccessId(overlayId, accessId);
            if (savedStyle != null)
            {
                UpdateLabelingStyle(labelingStyleOverlay, savedStyle);
            }

            return labelingStyleOverlay;

        }

        /// <summary>
        /// Gets the labeling style applied to a specified overlay.
        /// </summary>
        public static Dictionary<string, object> GetLabelingStyle(LayerOverlay overlay)
        {
            Dictionary<string, object> styles = new Dictionary<string, object>();

            TextStyle textStyle = ((ShapeFileFeatureLayer)overlay.Layers[0]).ZoomLevelSet.ZoomLevel10.DefaultTextStyle;
            styles["haloPen"] = textStyle.HaloPen.Color.AlphaComponent != 0;
            styles["mask"] = textStyle.Mask.IsActive;
            styles["overlapping"] = textStyle.OverlappingRule == LabelOverlappingRule.AllowOverlapping;
            styles["duplicate"] = textStyle.DuplicateRule.ToString();
            styles["drawingMargin"] = ((ShapeFileFeatureLayer)overlay.Layers[0]).DrawingMarginPercentage.ToString();
            styles["gridSize"] = "Small";
            foreach (var item in gridSizeConfigurations)
            {
                if (item.Value == textStyle.GridSize)
                {
                    styles["gridSize"] = item.Key;
                    break;
                }
            }

            return styles;
        }

        /// <summary>
        /// Updates the specified style to a specified overlay.
        /// </summary>
        private static void UpdateLabelingStyle(LayerOverlay overlay, Dictionary<string, string> styles)
        {
            string haloPen = styles["haloPen"];
            string mask = styles["mask"];
            string overlapping = styles["overlapping"];
            string duplicateRule = styles["duplicate"];
            string gridSize = styles["gridSize"];
            string drawingMargin = styles["drawingMargin"];

            if (overlay.Layers.Count > 0)
            {
                bool useHalopen = false;
                bool useMask = false;
                bool allowOverlapping = false;
                LabelDuplicateRule labelDuplicateRule = LabelDuplicateRule.OneDuplicateLabelPerQuadrant;
                double drawingMarginPercentage = 0;

                if (bool.TryParse(haloPen, out useHalopen) && bool.TryParse(mask, out useMask)
                    && bool.TryParse(overlapping, out allowOverlapping)
                    && Enum.TryParse<LabelDuplicateRule>(duplicateRule, out labelDuplicateRule)
                    && double.TryParse(drawingMargin, out drawingMarginPercentage)
                    && gridSizeConfigurations.ContainsKey(gridSize))
                {
                    int gridSizeValue = gridSizeConfigurations[gridSize];

                    foreach (string layerId in overlay.Layers.GetKeys())
                    {
                        ShapeFileFeatureLayer featureLayer = overlay.Layers[layerId] as ShapeFileFeatureLayer;
                        List<TextStyle> textStyles = new List<TextStyle>();
                        if (featureLayer.ZoomLevelSet.ZoomLevel10.CustomStyles.Count > 0)
                        {
                            ClassBreakStyle classBreakStyle = featureLayer.ZoomLevelSet.ZoomLevel10.CustomStyles[0] as ClassBreakStyle;
                            textStyles = classBreakStyle.ClassBreaks.Select(c => c.DefaultTextStyle).ToList();
                        }
                        else
                        {
                            textStyles.Add(featureLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle);
                        }
                        foreach (var textStyle in textStyles)
                        {
                            int red = textStyle.HaloPen.Color.RedComponent;
                            int green = textStyle.HaloPen.Color.GreenComponent;
                            int blue = textStyle.HaloPen.Color.BlueComponent;
                            int alpha = useHalopen ? 255 : 0;

                            textStyle.HaloPen = new GeoPen(GeoColor.FromArgb(alpha, red, green, blue), textStyle.HaloPen.Width);
                            textStyle.Mask.IsActive = useMask;
                            textStyle.OverlappingRule = allowOverlapping ? LabelOverlappingRule.AllowOverlapping : LabelOverlappingRule.NoOverlapping;
                            textStyle.DuplicateRule = labelDuplicateRule;
                            textStyle.GridSize = gridSizeValue;
                            featureLayer.DrawingMarginPercentage = drawingMarginPercentage;
                        }
                    }
                }
            }
        }

        private static ShapeFileFeatureLayer GetLabelingStyleParcelLayer()
        {
            ShapeFileFeatureLayer parcelLayer = new ShapeFileFeatureLayer(string.Format(@"{0}\{1}", GetBaseDirectory(), "Parcels.shp"));
            // Here shows how to apply text style to a layer

            /// We can use predefined style using the code like:
            //parcelLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle = TextStyles.Park1("X_REF");

            // Or creating a simple one using the code like:
            //parcelLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle = TextStyles.CreateSimpleTextStyle("X_REF", "Arial", 9, DrawingFontStyles.Regular, GeoColor.FromHtml("#d9ccbe"));

            // Or create a new one by ourselves
            parcelLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle = new TextStyle("X_REF", new GeoFont("Arail", 6, DrawingFontStyles.Regular), new GeoSolidBrush(GeoColor.FromHtml("#7b7b78")));

            parcelLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.DuplicateRule = LabelDuplicateRule.NoDuplicateLabels;
            parcelLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.GridSize = 1000;
            parcelLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.HaloPen = new GeoPen(GeoColor.SimpleColors.White, 1);
            parcelLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.Mask = new AreaStyle();
            parcelLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.OverlappingRule = LabelOverlappingRule.NoOverlapping;
            parcelLayer.ZoomLevelSet.ZoomLevel10.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;
            // Set DrawingMarginPercentage to a proper value to avoid some labels are cut-off
            parcelLayer.DrawingMarginPercentage = 300;

            return parcelLayer;
        }

        private static ShapeFileFeatureLayer GetLabelingStylePoisLayer()
        {
            ShapeFileFeatureLayer restaurantsLayer = new ShapeFileFeatureLayer(string.Format(@"{0}\{1}", GetBaseDirectory(), "Pois.shp"));
            restaurantsLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle = new TextStyle("Name", new GeoFont("Arail", 8, DrawingFontStyles.Bold), new GeoSolidBrush(GeoColor.FromHtml("#666666")));
            restaurantsLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.GridSize = 1000;
            restaurantsLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.HaloPen = new GeoPen(GeoColor.SimpleColors.White, 1);
            restaurantsLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.XOffsetInPixel = 5;
            restaurantsLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.Mask = new AreaStyle(new GeoPen(GeoColor.FromHtml("#999999"), 1), new GeoSolidBrush(new GeoColor(100, GeoColor.FromHtml("#eeeeee"))), PenBrushDrawingOrder.PenFirst);
            restaurantsLayer.ZoomLevelSet.ZoomLevel10.DefaultTextStyle.OverlappingRule = LabelOverlappingRule.NoOverlapping;
            restaurantsLayer.ZoomLevelSet.ZoomLevel10.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;
            // Set DrawingMarginPercentage to a proper value to avoid some labels are cut-off
            restaurantsLayer.DrawingMarginPercentage = 300;

            return restaurantsLayer;
        }

        private static ShapeFileFeatureLayer GetLabelingStreetLayer()
        {
            ShapeFileFeatureLayer streetLayer = new ShapeFileFeatureLayer(string.Format(@"{0}\{1}", GetBaseDirectory(), "Street.shp"));

            // Create a classBreakStyle for different type of roads
            ClassBreakStyle roadStyle = new ClassBreakStyle("Type");
            roadStyle.BreakValueInclusion = BreakValueInclusion.ExcludeValue;

            ClassBreak break1 = new ClassBreak();
            break1.Value = 0.9;
            break1.DefaultTextStyle = new TextStyle("ROAD_NAME", new GeoFont("Arial", 12, DrawingFontStyles.Regular), new GeoSolidBrush(GeoColor.FromHtml("#666666")));
            break1.DefaultTextStyle.HaloPen = new GeoPen(GeoColor.SimpleColors.White, 2);
            break1.DefaultTextStyle.Mask = new AreaStyle();
            roadStyle.ClassBreaks.Add(break1);

            ClassBreak break2 = new ClassBreak();
            break2.Value = 3.8;
            break2.DefaultTextStyle = new TextStyle("ROAD_NAME", new GeoFont("Arial", 8, DrawingFontStyles.Bold), new GeoSolidBrush(GeoColor.FromHtml("#666666")));
            break2.DefaultTextStyle.HaloPen = new GeoPen(GeoColor.SimpleColors.White, 1);
            break2.DefaultTextStyle.Mask = new AreaStyle();
            roadStyle.ClassBreaks.Add(break2);

            ClassBreak break3 = new ClassBreak();
            break3.Value = 7;
            break3.DefaultTextStyle = new TextStyle("ROAD_NAME", new GeoFont("Arial", 6, DrawingFontStyles.Regular), new GeoSolidBrush(GeoColor.FromHtml("#666666")));
            break3.DefaultTextStyle.Mask = new AreaStyle();
            roadStyle.ClassBreaks.Add(break3);

            streetLayer.ZoomLevelSet.ZoomLevel10.CustomStyles.Add(roadStyle);
            streetLayer.ZoomLevelSet.ZoomLevel10.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;
            // Set DrawingMarginPercentage to a proper value to avoid some labels are cut-off
            streetLayer.DrawingMarginPercentage = 200;

            return streetLayer;
        }

        public static Dictionary<string, string> GetSavedLabelStyleByAccessId(string overlayId, string accessId)
        {
            string styleFile = string.Format("{0}_{1}.json", accessId, overlayId);
            string styleFilePath = Path.Combine(GetBaseDirectory(), "Temp", styleFile);

            if (File.Exists(styleFilePath))
            {
                string content = File.ReadAllText(styleFilePath);
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
            }

            return null;
        }

        public static void SaveLabelStyle(string overlayId, Dictionary<string, string> newStyles, string accessId)
        {
            string styleFile = string.Format("{0}_{1}.json", accessId, overlayId);
            string styleFilePath = Path.Combine(GetBaseDirectory(), "Temp", styleFile);

            using (StreamWriter streamWriter = new StreamWriter(styleFilePath, false))
            {
                streamWriter.WriteLine(JsonConvert.SerializeObject(newStyles));
            }
        }

        private static string GetBaseDirectory()
        {
            Uri uri = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            string rootDirectory = Path.GetDirectoryName(Path.GetDirectoryName(uri.LocalPath));

            return Path.Combine(rootDirectory, "App_Data"); ;
        }
    }
}