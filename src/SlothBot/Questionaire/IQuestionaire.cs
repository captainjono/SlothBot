using System;
using System.Collections.Generic;
using System.Text;
using SlothBot.MessagingPipeline;

namespace SlothBot.Questionaire
{
    public interface IQuestion
    {
        string Key { get; }
        string Ask { get; }
        Func<string, QuestionResult> Evaluate { get; }
    }

    public class QuestionResult
    {
        public string Response { get; set; }
        public IQuestionAdapter[] ResponseQuestions { get; set; }

        public bool WasSuccessful { get; set; }

        public static QuestionResult Ok(string response = "Ok!", params IQuestionAdapter[] questions)
        {
            return new QuestionResult()
            {
                Response = response,
                ResponseQuestions = questions,
                WasSuccessful = true
            };
        }

        public static QuestionResult Ok(IQuestionaire questions)
        {
            return Ok(questions: Questions.ForQuestionaire(questions));
        }

        public static QuestionResult Ok(IQuestion question)
        {
            return Ok(questions: Questions.ForQuestionaire(question));
        }


        public static QuestionResult Error(string response)
        {
            return new QuestionResult()
            {
                Response = response,
                WasSuccessful = false
            };
        }
    }
    
    public interface IQuestionaire 
    {
        string Description { get; }

        /// <summary>
        /// null if the questionaaire is complete, otherwise the question
        /// to be answered
        /// </summary>
        IQuestionAdapter Current { get; }
        QuestionaireResult AnswerQuestionWith(IncomingMessage message);
        bool IsComplete { get; }
    }

    public class QuestionaireResult
    {
        public bool WasSuccessful { get; set; }
        public string Reply { get; set; }

        public static QuestionaireResult Ok(string response = "Ok!")
        {
            return new QuestionaireResult()
            {
                Reply = response,
                WasSuccessful = true
            };
        }

        public static QuestionaireResult Error(string response)
        {
            return new QuestionaireResult()
            {
                Reply = response,
                WasSuccessful = false
            };
        }
    }
}
