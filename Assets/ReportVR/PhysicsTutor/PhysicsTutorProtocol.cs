// Shared HTTP contract from AI物理导师/ai-physics-tutor/prototype_delivery/unity.
using System;

namespace PhysicsTutorPrototype
{
    [Serializable] public class TutorMessage { public string role; public string content; }
    [Serializable] public class StudentState
    {
        public string task = "";
        public string observation = "";
        public string attempted_reasoning = "";
    }
    [Serializable] public class TutorRequest
    {
        public string question;
        public string mode = "teach";
        public int hint_level = 1;
        public StudentState student_state = new StudentState();
        public TutorMessage[] history = new TutorMessage[0];
    }
    [Serializable] public class TutorAnswer
    {
        public string status, reply, unit;
        public string[] evidence_ids;
    }
    [Serializable] public class TutorEvidence
    {
        public string id, title, source_url, source_section, source_revision, attribution, review_status, text, license;
        public string misconceptions;
    }
    [Serializable] public class TutorResponse
    {
        public bool ok;
        public TutorAnswer answer;
        public TutorEvidence[] evidence;
        public double response_seconds;
        public string error, review_status, scope;
    }
}
