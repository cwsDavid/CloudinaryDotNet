﻿using NUnit.Framework;
using System.Collections.Generic;

namespace CloudinaryDotNet.Test
{
    public partial class ApiTest
    {
        [Test]
        public void WithLiteral()
        {
            Transformation transformation = new Transformation().IfCondition("w_lt_200").Crop("fill").Height(120).Width(80);
            string sTransform = transformation.ToString();
            //"should include the if parameter as the first component in the transformation string"
            Assert.AreEqual(sTransform.IndexOf("if"), 0);
            //"should be proper transformation string"
            Assert.AreEqual("if_w_lt_200,c_fill,h_120,w_80", sTransform);

            transformation = new Transformation().Crop("fill").Height(120).IfCondition("w_lt_200").Width(80);
            sTransform = transformation.ToString();
            //"should include the if parameter as the first component in the transformation string"
            Assert.AreEqual(sTransform.IndexOf("if"), 0);
            //"components should be in proper order"
            Assert.AreEqual("if_w_lt_200,c_fill,h_120,w_80", sTransform);

            transformation = new Transformation().IfCondition("w_lt_200").Crop("fill").Height(120).Width(80).
                                          Chain().IfCondition("w_gt_400").Crop("fit").Height(150).Width(150).
                                          Chain().Effect("sepia");
            sTransform = transformation.ToString();
            //"should allow multiple conditions when chaining transformations"
            Assert.AreEqual("if_w_lt_200,c_fill,h_120,w_80/if_w_gt_400,c_fit,h_150,w_150/e_sepia", sTransform);
        }

        [Test]
        public void LiteralWithSpaces()
        {
            Transformation transformation = new Transformation().IfCondition("w < 200").Crop("fill").Height(120).Width(80);
            string sTransform = transformation.ToString();
            //"should translate operators"
            Assert.AreEqual("if_w_lt_200,c_fill,h_120,w_80", sTransform);
        }

        [Test]
        public void EndIf()
        {
            Transformation transformation = new Transformation().IfCondition("w_lt_200").Crop("fill").Height(120).Width(80).Effect("sharpen")
                .Chain().Effect("brightness", 50)
                .Chain().Effect("shadow").Color("red")
                .EndIf();
            string sTransform = transformation.ToString();
            Assert.True(sTransform.EndsWith("if_end"), "should include the if_end as the last parameter in its component");
            //"should be proper transformation string"
            Assert.AreEqual("if_w_lt_200/c_fill,e_sharpen,h_120,w_80/e_brightness:50/co_red,e_shadow/if_end", sTransform);
        }

        [Test]
        public void IfElse()
        {
            List<Transformation> transformations = new List<Transformation>()
            {
                new Transformation().IfCondition("w_lt_200").Crop("fill").Height(120).Width(80),
                new Transformation().IfElse().Crop("fill").Height(90).Width(100)
            };
            var transformation = new Transformation(transformations);
            var sTransform = transformation.ToString();
            //"should support if_else with transformation parameters"
            Assert.AreEqual("if_w_lt_200,c_fill,h_120,w_80/if_else,c_fill,h_90,w_100", sTransform);

            transformations = new List<Transformation>()
            {
                new Transformation().IfCondition("w_lt_200"),
                new Transformation().Crop("fill").Height(120).Width(80),
                new Transformation().IfElse(),
                new Transformation().Crop("fill").Height(90).Width(100)
            };
            transformation = new Transformation(transformations);
            sTransform = transformation.ToString();
            Assert.True(sTransform.Contains("/if_else/"), "if_else should be without any transformation parameters");
            //"should be proper transformation string"
            Assert.AreEqual("if_w_lt_200/c_fill,h_120,w_80/if_else/c_fill,h_90,w_100", sTransform);
        }

        [Test]
        public void ChainedConditions()
        {
            Transformation transformation = new Transformation().IfCondition().AspectRatio("gt", "3:4").Then().Width(100).Crop("scale");
            //"passing an operator and a value adds a condition"
            Assert.AreEqual("if_ar_gt_3:4,c_scale,w_100", transformation.ToString());

            transformation = new Transformation().IfCondition().AspectRatio("gt", "3:4").And().Width("gt", 100).Then().Width(50).Crop("scale");
            //"should chaining condition with `and`"
            Assert.AreEqual("if_ar_gt_3:4_and_w_gt_100,c_scale,w_50", transformation.ToString());

            transformation = new Transformation().IfCondition().AspectRatio("gt", "3:4").And().Width("gt", 100).Or().Width("gt", 200).Then().Width(50).Crop("scale");
            //"should chain conditions with `or`"
            Assert.AreEqual("if_ar_gt_3:4_and_w_gt_100_or_w_gt_200,c_scale,w_50", transformation.ToString());

            transformation = new Transformation().IfCondition().AspectRatio(">", "3:4").And().Width("<=", 100).Or().Width("gt", 200).Then().Width(50).Crop("scale");
            //"should translate operators"
            Assert.AreEqual("if_ar_gt_3:4_and_w_lte_100_or_w_gt_200,c_scale,w_50", transformation.ToString());

            transformation = new Transformation().IfCondition().AspectRatio(">", "3:4").And().Width("<=", 100).Or().Width(">", 200).Then().Width(50).Crop("scale");
            //"should translate operators"
            Assert.AreEqual("if_ar_gt_3:4_and_w_lte_100_or_w_gt_200,c_scale,w_50", transformation.ToString());

            transformation = new Transformation().IfCondition().AspectRatio(">=", "3:4").And().PageCount(">=", 100).Or().PageCount("!=", 0).Then().Width(50).Crop("scale");
            //"should translate operators"
            Assert.AreEqual("if_ar_gte_3:4_and_pc_gte_100_or_pc_ne_0,c_scale,w_50", transformation.ToString());
        }

        [Test]
        public void ShouldSupportAndTranslateOperators()
        {
            string allOperators =
                            "if_" +
                            "w_eq_0_and" +
                            "_h_ne_0_or" +
                            "_ar_lt_0_and" +
                            "_pc_gt_0_and" +
                            "_fc_lte_0_and" +
                            "_w_gte_0" +
                            ",e_grayscale";
            //"should support and translate operators:  '=', '!=', '<', '>', '<=', '>=', '&&', '||'"
            Assert.AreEqual(allOperators, new Transformation().IfCondition()
                            .Width("=", 0).And()
                            .Height("!=", 0).Or()
                            .AspectRatio("<", "0").And()
                            .PageCount(">", 0).And()
                            .FaceCount("<=", 0).And()
                            .Width(">=", 0)
                            .Then().Effect("grayscale").ToString());

            Assert.AreEqual(allOperators, new Transformation().IfCondition("w = 0 && height != 0 || aspectRatio < 0 and pageCount > 0 and faceCount <= 0 and width >= 0")
                        .Effect("grayscale")
                        .ToString());
        }

        [Test]
        public void EndIf2()
        {
            Transformation transformation = new Transformation().IfCondition().Width("gt", 100).And().Width("lt", 200).Then().Width(50).Crop("scale").EndIf();
            //"should serialize to 'if_end'"
            Assert.AreEqual("if_w_gt_100_and_w_lt_200/c_scale,w_50/if_end", transformation.ToString());

            transformation = new Transformation().IfCondition().Width("gt", 100).And().Width("lt", 200).Then().Width(50).Crop("scale").EndIf();
            //"force the if clause to be chained"
            Assert.AreEqual("if_w_gt_100_and_w_lt_200/c_scale,w_50/if_end", transformation.ToString());

            transformation = new Transformation().IfCondition().Width("gt", 100).And().Width("lt", 200).Then().Width(50).Crop("scale").IfElse().Width(100).Crop("crop").EndIf();
            //"force the if_else clause to be chained"
            Assert.AreEqual("if_w_gt_100_and_w_lt_200/c_scale,w_50/if_else/c_crop,w_100/if_end", transformation.ToString());
        }
    }
}
