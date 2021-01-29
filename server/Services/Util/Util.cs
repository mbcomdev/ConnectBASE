using BpNT;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace connectBase.Services.Util
{
    public class Util
    {
        /// <summary>
        /// convert values Dictionary <string,object> provided by mvcController to 'normal' values anyone can work with
        /// </summary>
        /// <param name="mvcData"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ConvertMVCDirectory(Dictionary<string, object> mvcData)
        {
            Dictionary<string, object> convertedData = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> kvp in mvcData)
            {
                object convertedValue;
                /* handle value is null */
                if (kvp.Value == null)
                {
                    convertedValue = null;
                }
                /* handle all other cases */
                else
                {
                    /* mvc controller converts json object to JsonElement so we have to cast it */
                    JsonElement value = (JsonElement)kvp.Value;
                    switch (value.ValueKind)
                    {
                        case JsonValueKind.Number:
                            convertedValue = value.GetInt32();
                            break;
                        case JsonValueKind.False:
                            convertedValue = value.GetBoolean();
                            break;
                        case JsonValueKind.True:
                            convertedValue = value.GetBoolean();
                            break;
                        case JsonValueKind.String:
                            convertedValue = value.GetString();
                            break;
                        default:
                            // TODO Should be updated to a custom exception
                            throw new Exception();
                    }
                }
                /* add converted value the Dictionary with the converted values */
                convertedData.Add(kvp.Key, convertedValue);
            }
            return convertedData;
        }



        #region util funktions to help access and edit com dataSet data

        /// <summary>
        /// set Dataset to specific row of table 
        /// this funktion will remove the keys from the Dictionary so use a copy if you need this fields later on
        /// </summary>
        /// <param name="dataset"></param>
        /// <param name="index"></param>
        /// <param name="valueList"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static BpNT.AutoDataSet GoToSpecificRowOfTable(BpNT.AutoDataSet dataset, string index, Dictionary<string, object> valueList, string tableName)
        {
            dataset.Indices[index].Select();
            dataset.SetKey();
            /* iterate through table key atributes specified by index and set them to in valueList specified values */
            for (int i = 0; i < dataset.Indices[index].IndexFields.Count; i++)
            {
                string field = dataset.Indices[index].IndexFields[i].FieldInfo.Name;
                if (!valueList.ContainsKey(field))
                {
                    throw new Exception("field: " + field + "is not specified but needs to be specefied for table: " + tableName + " with index: " + index + "!");
                }
                dataset.Fields[field].AsString = Convert.ToString(valueList[field]);
                valueList.Remove(field);
            }
            /* try to go to table row that matches the bevore specified keys */
            if (!dataset.GotoKey())
            {
                throw new Exception("could not find a matching table row for the providet key values with index " + index);
            };
            return dataset;
        }

        /// <summary>
        /// update before specified table row of dataset to as map provided values
        /// </summary>
        /// <param name="dataset"></param>
        /// <param name="newValues"></param>
        /// <param name="returnValues"></param>
        /// <returns></returns>
        public static string UpdateSpecifiedRow(BpNT.AutoDataSet dataset, Dictionary<string, object> newValues, List<string> returnValues)
        {
            /* make dataset editable */
            dataset.Edit();
            /* iterate through provided values and update the dataset */
            foreach (KeyValuePair<string, object> kvp in newValues)
            {
                WritePropertyValue(dataset, kvp.Key, kvp.Value);
            }
            /* push bevore made changes to dataset */
            dataset.Post();
            /* iterate through bevore changed atributes and read theyr values from dataset */
            Dictionary<string, object> updatedValues = new Dictionary<string, object>();
            foreach (string fieldName in returnValues)
            {
                updatedValues.Add(fieldName, ReadPropertyValue(dataset, fieldName));
            }
            /* return changed values */
            return JsonConvert.SerializeObject(updatedValues);
        }

        /// <summary>
        /// Method to return the value of a specific field in a before selected Row
        /// </summary>
        /// <param name="autoDataSet"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static object ReadPropertyValue(BpNT.IAutoDataSet autoDataSet, string fieldName)
        {
            return autoDataSet.Fields[fieldName].FieldInfo.Art switch
            {
                FeldDefArt.fdaChar => autoDataSet.Fields[fieldName].Value,
                FeldDefArt.fdaByte => autoDataSet.Fields[fieldName].Value,
                FeldDefArt.fdaBoolean => autoDataSet.Fields[fieldName].AsBoolean,
                FeldDefArt.fdaSmallInt => autoDataSet.Fields[fieldName].Value,
                FeldDefArt.fdaAutoInc => autoDataSet.Fields[fieldName].AsInteger,
                FeldDefArt.fdaInteger => autoDataSet.Fields[fieldName].AsInteger,
                FeldDefArt.fdaSingle => autoDataSet.Fields[fieldName].Value,
                FeldDefArt.fdaDouble => autoDataSet.Fields[fieldName].AsFloat,
                FeldDefArt.fdaDate => autoDataSet.Fields[fieldName].AsDateTime,
                FeldDefArt.fdaTime => autoDataSet.Fields[fieldName].AsDateTime,
                FeldDefArt.fdaDateTime => autoDataSet.Fields[fieldName].AsDateTime,
                FeldDefArt.fdaString => autoDataSet.Fields[fieldName].AsString,
                FeldDefArt.fdaBild => "Nur schreibender Zugriff!",
                FeldDefArt.fdaInfo => autoDataSet.Fields[fieldName].Text,
                FeldDefArt.fdaUnicodeString => autoDataSet.Fields[fieldName].AsString,
                _ => throw new Exception(autoDataSet.Fields[fieldName].FieldType),// TODO Should be updated to a custom exception
            };
        }

        /// <summary>
        /// Similar to GoToKey but selects all rows between start and end
        /// </summary>
        /// <param name="dataset"></param>
        /// <param name="indexFields"></param>
        /// <returns></returns>
        public static BpNT.AutoDataSet SetRange(AutoDataSet dataset, Dictionary<string, string> indexFields)
        {
            dataset.SetRangeStart();
            foreach (KeyValuePair<string, string> kvp in indexFields)
            {
                dataset.Fields[kvp.Key].AsString = kvp.Value.Split("-")[0];
            }
            dataset.SetRangeEnd();
            foreach (KeyValuePair<string, string> kvp in indexFields)
            {
                dataset.Fields[kvp.Key].AsString = kvp.Value.Split("-")[1];
            }
            dataset.ApplyRange();
            return dataset;
        }

        /// <summary>
        /// Method to set the value of a specific field in a before selected Row
        /// Before a field can be set autoDataSet.Edit() or autoDataSet.Append() must be called
        /// Values are written with autoDataSet.Post()
        /// COMExceptions after Edit or Append is called till Post() is called should be catched and the autoDataSet.Cancel() should be called.
        /// </summary>
        /// <param name="autoDataSet"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        public static void WritePropertyValue(BpNT.AutoDataSet autoDataSet, string fieldName, object value)
        {
            switch (autoDataSet.Fields[fieldName].FieldInfo.Art)
            {
                case FeldDefArt.fdaChar:
                    autoDataSet.Fields[fieldName].Value = Convert.ToChar(value);
                    break;
                case FeldDefArt.fdaByte:
                    autoDataSet.Fields[fieldName].Value = Convert.ToByte(value);
                    break;
                case FeldDefArt.fdaBoolean:
                    autoDataSet.Fields[fieldName].AsBoolean = Convert.ToBoolean(value);
                    break;
                case FeldDefArt.fdaSmallInt:
                    autoDataSet.Fields[fieldName].Value = Convert.ToInt16(value);
                    break;
                case FeldDefArt.fdaAutoInc:
                case FeldDefArt.fdaInteger:
                    autoDataSet.Fields[fieldName].AsInteger = Convert.ToInt32(value);
                    break;
                case FeldDefArt.fdaSingle:
                    autoDataSet.Fields[fieldName].Value = Convert.ToSingle(value);
                    break;
                case FeldDefArt.fdaDouble:
                    autoDataSet.Fields[fieldName].AsFloat = Convert.ToDouble(value);
                    break;
                case FeldDefArt.fdaDate:
                case FeldDefArt.fdaTime:
                case FeldDefArt.fdaDateTime:
                    autoDataSet.Fields[fieldName].AsDateTime = Convert.ToDateTime(value);
                    break;
                case FeldDefArt.fdaString:
                    autoDataSet.Fields[fieldName].AsString = Convert.ToString(value);
                    break;
                case FeldDefArt.fdaBild:
                    AutoImage image = (AutoImage)autoDataSet.Fields[fieldName].GetEditObject(EditTyp.etImage);
                    try
                    {
                        image.LoadFromFile((string)value);
                        image.Save();
                    }
                    catch
                    {
                        image.Clear();
                    }
                    break;
                case FeldDefArt.fdaInfo:
                    autoDataSet.Fields[fieldName].Text = Convert.ToString(value);
                    break;
                case FeldDefArt.fdaUnicodeString:
                    autoDataSet.Fields[fieldName].AsString = Convert.ToString(value);
                    break;
                default:
                    throw new Exception(autoDataSet.Fields[fieldName].FieldType);
            }
        }

        public bool GoToKey(AutoDataSet dataset, Dictionary<string, string> indexFields)
        {
            dataset.SetKey();
            foreach (KeyValuePair<string, string> kvp in indexFields)
            {
                dataset.Fields[kvp.Key].AsString = kvp.Value;
            }
            return dataset.GotoKey();
        }

        #endregion
    }
}
