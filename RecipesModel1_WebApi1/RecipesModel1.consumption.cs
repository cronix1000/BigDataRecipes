﻿// This file was auto-generated by ML.NET Model Builder.
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
public partial class RecipesModel1
{
    /// <summary>
    /// model input class for RecipesModel1.
    /// </summary>
    #region model input class
    public class ModelInput
    {
        [LoadColumn(0)]
        [ColumnName(@"user_id")]
        public float User_id { get; set; }

        [LoadColumn(1)]
        [ColumnName(@"recipe_id")]
        public float Recipe_id { get; set; }

        [LoadColumn(3)]
        [ColumnName(@"rating")]
        public float Rating { get; set; }

    }

    #endregion

    /// <summary>
    /// model output class for RecipesModel1.
    /// </summary>
    #region model output class
    public class ModelOutput
    {
        [ColumnName(@"user_id")]
        public float User_id { get; set; }

        [ColumnName(@"recipe_id")]
        public float Recipe_id { get; set; }

        [ColumnName(@"rating")]
        public float Rating { get; set; }

        [ColumnName(@"Score")]
        public float Score { get; set; }

    }

    #endregion

    private static string MLNetModelPath = Path.GetFullPath("RecipesModel1.mlnet");

    public static readonly Lazy<PredictionEngine<ModelInput, ModelOutput>> PredictEngine = new Lazy<PredictionEngine<ModelInput, ModelOutput>>(() => CreatePredictEngine(), true);


    private static PredictionEngine<ModelInput, ModelOutput> CreatePredictEngine()
    {
        var mlContext = new MLContext();
        ITransformer mlModel = mlContext.Model.Load(MLNetModelPath, out var _);
        return mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);
    }

    /// <summary>
    /// Use this method to predict on <see cref="ModelInput"/>.
    /// </summary>
    /// <param name="input">model input.</param>
    /// <returns><seealso cref=" ModelOutput"/></returns>
    public static ModelOutput Predict(ModelInput input)
    {
        var predEngine = PredictEngine.Value;
        return predEngine.Predict(input);
    }

}
