// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
// All rights reserved.
//------------------------------------------------------------------------------
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the ids-adi.org nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//------------------------------------------------------------------------------
// THIS SOFTWARE IS PROVIDED BY ids-adi.org ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ids-adi.org BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Workflow.Activities.Rules;
using System.Workflow.ComponentModel.Serialization;
using System.Xml;


namespace org.iringtools.adapter.rules
{
    /// <summary>
    /// Summary description for RuleEngine
    /// </summary>
    public class RuleEngine
    {
        public RuleEngine()
        {
        }

        public List<DataTransferObject> dtoList{get;set;}

        public System.Collections.IEnumerator enumerator{get;set;}

        public DataTransferObject currentItem{get;set;}

      /// <summary>
      /// Applies the rules for a collection of items.
      /// </summary>
      /// <param name="commonDTOs">Collection of commonDTOs on which the rules will be applied.</param>
      /// <param name="ruleFilePath">Path of Ruleset file.</param>
      /// <returns>Returns the modified CommonDTOList after application of rules.</returns>
        public List<DataTransferObject> RuleSetForCollection(List<DataTransferObject> commonDTOs, string ruleFilePath)
        {
            try
            {
                RuleSet ruleSet = new RuleSet();
                CodeThisReferenceExpression thisRef = new CodeThisReferenceExpression();

                CodePropertyReferenceExpression enumerator = new CodePropertyReferenceExpression(thisRef, "enumerator");
                CodePropertyReferenceExpression dtoList = new CodePropertyReferenceExpression(thisRef, "dtoList");
                CodePropertyReferenceExpression currentItem = new CodePropertyReferenceExpression(thisRef, "currentItem");
                CodeMethodReferenceExpression getEnumerator = new CodeMethodReferenceExpression(thisRef, "GetEnumerator");
                CodePropertyReferenceExpression current = new CodePropertyReferenceExpression(thisRef, "Current");
                CodeMethodReferenceExpression moveNext = new CodeMethodReferenceExpression(thisRef, "MoveNext");

                #region Rule Enumeration
                Rule ruleEnumeration = new Rule("Enumeration");

                CodeAssignStatement assignStatementEnumeration = new CodeAssignStatement();
                CodeMethodInvokeExpression invokeExpressionEnumeration = new CodeMethodInvokeExpression();
                invokeExpressionEnumeration.Method = getEnumerator;
                assignStatementEnumeration.Left = enumerator;
                assignStatementEnumeration.Right = invokeExpressionEnumeration;
                getEnumerator.TargetObject = dtoList;
                RuleStatementAction statementActionEnumeration = new RuleStatementAction(assignStatementEnumeration);
                ruleEnumeration.ThenActions.Add(statementActionEnumeration);

                CodeBinaryOperatorExpression binaryOperatorTrue = new CodeBinaryOperatorExpression();
                binaryOperatorTrue.Operator = CodeBinaryOperatorType.ValueEquality;
                binaryOperatorTrue.Left = new CodePrimitiveExpression("1");
                binaryOperatorTrue.Right = new CodePrimitiveExpression("1");
                ruleEnumeration.Condition = new RuleExpressionCondition(binaryOperatorTrue);

                ruleEnumeration.Priority = 2;
                ruleSet.Rules.Add(ruleEnumeration);
                #endregion

                #region Rule Move Next
                Rule ruleMoveNext = new Rule("MoveNext");

                CodeCastExpression castExpressionMoveNext = new CodeCastExpression();
                CodeTypeReference typeReference = new CodeTypeReference("org.ids_adi.camelot.dataLayer.Model.CommonDTO");
                castExpressionMoveNext.TargetType = typeReference;
                castExpressionMoveNext.Expression = current;
                CodeAssignStatement assignStatementMoveNext = new CodeAssignStatement();
                assignStatementMoveNext.Left = (CodePropertyReferenceExpression)currentItem;
                assignStatementMoveNext.Right = castExpressionMoveNext;
                current.TargetObject = enumerator;
                RuleStatementAction statementActionMoveNext = new RuleStatementAction(assignStatementMoveNext);
                ruleMoveNext.ThenActions.Add(statementActionMoveNext);

                CodeMethodInvokeExpression methodInvokeMoveNext = new CodeMethodInvokeExpression();
                methodInvokeMoveNext.Method = moveNext;
                moveNext.TargetObject = enumerator;
                ruleMoveNext.Condition = new RuleExpressionCondition(methodInvokeMoveNext);

                ruleMoveNext.Priority = 1;
                ruleSet.Rules.Add(ruleMoveNext);
                #endregion

                #region Rule Update enumerator
                Rule ruleUpdateEnumerator = new Rule("UpdateEnumerator");

                RuleUpdateAction updateAction = new RuleUpdateAction("this/enumerator");
                ruleUpdateEnumerator.ThenActions.Add(updateAction);
                ruleUpdateEnumerator.ElseActions.Add(updateAction);

                CodeBinaryOperatorExpression binaryOperatorCurrentItemTrue = new CodeBinaryOperatorExpression();
                binaryOperatorCurrentItemTrue.Operator = CodeBinaryOperatorType.ValueEquality;
                binaryOperatorCurrentItemTrue.Left = currentItem;
                binaryOperatorCurrentItemTrue.Right = currentItem;
                ruleUpdateEnumerator.Condition = new RuleExpressionCondition(binaryOperatorCurrentItemTrue);

                ruleUpdateEnumerator.Priority = -1;
                ruleSet.Rules.Add(ruleUpdateEnumerator);
                #endregion

                WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();

                // Deserialize from a .rules file.
                XmlTextReader rulesReader = new XmlTextReader(ruleFilePath);
                serializer = new WorkflowMarkupSerializer();
                RuleSet ruleSetFromFile = (RuleSet)serializer.Deserialize(rulesReader);
                rulesReader.Close();

                foreach (Rule rule in ruleSetFromFile.Rules)
                {
                    ruleSet.Rules.Add(rule);
                }

                this.dtoList = commonDTOs;

                RuleValidation validation = new RuleValidation(typeof(RuleEngine), null);
                RuleExecution execution = new RuleExecution(validation, this);
                ruleSet.Execute(execution);

                return this.dtoList;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

      /// <summary>
        /// Applies the rules for a single item.
      /// </summary>
        /// <param name="commonDTO">CommonDTO on which the rules will be applied.</param>
        /// <param name="ruleFilePath">Path of Ruleset file.</param>
      /// <returns>Returns the modified CommonDTO after the application of rules.</returns>
        public DataTransferObject RuleSetForSingleItem(DataTransferObject commonDTO, string ruleFilePath)
        {
            try
            {
                RuleSet ruleSet = new RuleSet();
                WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();

                // Deserialize from a .rules file.
                XmlTextReader rulesReader = new XmlTextReader(ruleFilePath);
                serializer = new WorkflowMarkupSerializer();
                ruleSet = (RuleSet)serializer.Deserialize(rulesReader);
                rulesReader.Close();

                this.currentItem = commonDTO;

                RuleValidation validation = new RuleValidation(typeof(RuleEngine), null);
                RuleExecution execution = new RuleExecution(validation, this);
                ruleSet.Execute(execution);

                return this.currentItem;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
