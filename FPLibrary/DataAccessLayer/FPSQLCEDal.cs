﻿using System;
using System.Data.SqlServerCe;
using System.Windows.Forms;
using GriauleFingerprintLibrary.DataTypes;

namespace FPLibrary.DataAccessLayer
{
  /// <summary>
  /// Implements the Fingerprint library data access layer for a SQL Ce database
  /// </summary>
  public class FPSQLCEDal : FPDal
  {
    public FPSQLCEDal()
    {
      Connection = new SqlCeConnection();
    }
    public FPSQLCEDal(string dbPath, string password)
    {
      Connection = GenericFactoryHelper.GetSQLCeConnection(dbPath, password);
    }
    #region Methods
    protected override void InternalSaveTemplate(byte fingerCode, GriauleFingerprintLibrary.DataTypes.FingerprintTemplate fingerPrintTemplate)
    {
      base.InternalSaveTemplate(fingerCode, fingerPrintTemplate);
      using (SqlCeCommand cmd = new SqlCeCommand())
      {
        try
        {
          cmd.Connection = (SqlCeConnection)Connection;
          string cmdStr = String.Format("SELECT COUNT(*) n FROM [{0}] WHERE [{1}]=@individual AND [{2}]=@finger",
                                        TemplateTable, IndividualFieldName, FingerFieldName);
          cmd.CommandText = cmdStr;
          cmd.Parameters.AddWithValue("@individual", ActiveIndividual);
          cmd.Parameters.AddWithValue("@finger", fingerCode);
          int cnt = (int)cmd.ExecuteScalar();
          if (cnt > 0) //Finger template already exists, update
          {
            cmdStr = String.Format("UPDATE [{0}] SET [{1}]=@template, SET [{2}]=@quality WHERE [{3}]=@individual AND [{4}]=@finger",
                                  TemplateTable, TemplateFieldName, QualityFieldName, IndividualFieldName, FingerFieldName);
          }
          else //Finger template doesn't exist, insert
          {
            cmdStr = String.Format("INSERT INTO [{0}] ([{1}],[{2}],[{3}],[{4}]) VALUES (@individual,@finger,@template,@quality)",
                                  TemplateTable, IndividualFieldName, FingerFieldName, TemplateFieldName, QualityFieldName);
          }
          cmd.CommandText = cmdStr;
          //cmd.Parameters.AddWithValue("@individual", ActiveIndividual); //Already set
          //cmd.Parameters.AddWithValue("@finger", fingerCode);           //Already set
          cmd.Parameters.AddWithValue("@template", fingerPrintTemplate.Buffer);
          cmd.Parameters.AddWithValue("@quality", fingerPrintTemplate.Quality);
          int rowsAffected = cmd.ExecuteNonQuery();
          if (rowsAffected == 0)
          {
            throw new FPLibraryException(FPLibraryException.LibraryDalError, String.Format("Template not saved for individual {0} finger {1}", ActiveIndividual, fingerCode));
          }
        }
        catch (Exception ex)
        {
          MessageBox.Show(ex.Message);
        }
      }
    }
    protected override System.Data.IDataReader InternalGetFingerTemplates()
    {
      using (SqlCeCommand cmd = new SqlCeCommand())
      {
        try
        {
          cmd.Connection = (SqlCeConnection)Connection;
          string cmdStr = String.Format("SELECT [{0}],[{1}],[{2}] FROM [{3}] WHERE [{4}]=@individual",
                              FingerFieldName, TemplateFieldName, QualityFieldName, TemplateTable, IndividualFieldName);
          cmd.CommandText = cmdStr;
          cmd.Parameters.AddWithValue("@individual", ActiveIndividual);
          return cmd.ExecuteReader();
        }
        catch (Exception ex)
        {
          MessageBox.Show(ex.Message);
          return null;
        }
      }
    }
    protected override System.Data.IDataReader InternalGetTemplates()
    {
      using (SqlCeCommand cmd = new SqlCeCommand())
      {
        try
        {
          cmd.Connection = (SqlCeConnection)Connection;
          string cmdStr = String.Format("SELECT [{0}],[{1}],[{2}],[{3}] FROM [{4}] ORDER BY [{0}],[{1}]",
                              IndividualFieldName, FingerFieldName, TemplateFieldName, QualityFieldName, TemplateTable);
          cmd.CommandText = cmdStr;
          return cmd.ExecuteReader();
        }
        catch (Exception ex)
        {
          MessageBox.Show(ex.Message);
          return null;
        }
      }
    }
    protected override bool IndividualsIsEqual(object individualA, object individualB)
    {
      return Convert.ToInt32(individualA)==Convert.ToInt32(individualB);
    }
    protected override FingerprintRawImage InternalGetFingerprintImage(Fingers finger)
    {
      return base.InternalGetFingerprintImage(finger);
    }
    #endregion
  }
}
