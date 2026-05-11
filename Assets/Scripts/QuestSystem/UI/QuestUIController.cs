using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace QuestSystem {
    public class QuestUIController : MonoBehaviour {
        public QuestManager questManager;

        [Header("UI Targets")]
        public TMP_Text tmpText;
        public Text uiText;

        [Header("Layout")]
        public bool useRichText = true;
        public bool showTitle = true;
        public bool showDescription = true;
        public bool showObjectiveNumbers = true;
        public bool showDistanceForLocation = true;
        public bool showStayTimerForLocation = true;

        [Header("Labels")]
        public string completedLabel = "DONE";
        public string activeLabel = "ACTIVE";
        public string pendingLabel = "TODO";

        [Header("Colors")]
        public string titleColor = "#F5D66B";
        public string descriptionColor = "#CFCFCF";
        public string completeColor = "#78D99C";
        public string activeColor = "#F0B26B";
        public string pendingColor = "#BFBFBF";

        private void OnEnable() {
            if (questManager != null) {
                questManager.onQuestProgressChanged.AddListener(Refresh);
                questManager.onQuestStarted.AddListener(Refresh);
                questManager.onQuestCompleted.AddListener(Refresh);
            }

            Refresh();
        }

        private void OnDisable() {
            if (questManager != null) {
                questManager.onQuestProgressChanged.RemoveListener(Refresh);
                questManager.onQuestStarted.RemoveListener(Refresh);
                questManager.onQuestCompleted.RemoveListener(Refresh);
            }
        }

        public void Refresh() {
            if (questManager == null) {
                SetText("No Quest Manager");
                return;
            }

            SetText(BuildQuestText());
        }

        private string BuildQuestText() {
            QuestManager.QuestDefinition quest = questManager.GetActiveQuest();
            if (quest == null) {
                return "No active quest.";
            }

            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            if (showTitle) {
                builder.Append(ApplyColor(quest.title, titleColor));
                builder.AppendLine();
            }

            if (showDescription && !string.IsNullOrEmpty(quest.description)) {
                builder.Append(ApplyColor(quest.description, descriptionColor));
                builder.AppendLine();
            }

            int count = questManager.GetObjectiveCount();
            for (int i = 0; i < count; i++) {
                QuestManager.ObjectiveStatus status = questManager.GetObjectiveStatus(i);
                string linePrefix = showObjectiveNumbers ? (i + 1).ToString() + ". " : "- ";
                string stateLabel = GetStateLabel(status);
                string stateColor = GetStateColor(status);

                builder.Append(linePrefix);
                builder.Append(ApplyColor("[" + stateLabel + "]", stateColor));
                builder.Append(" ");
                builder.Append(status.title);

                string progressText = GetProgressText(status);
                if (!string.IsNullOrEmpty(progressText)) {
                    builder.Append(" ");
                    builder.Append(progressText);
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }

        private string GetStateLabel(QuestManager.ObjectiveStatus status) {
            if (status.completed) {
                return completedLabel;
            }

            if (status.type == QuestManager.QuestObjectiveType.ReachLocation && status.distance <= status.reachRadius) {
                return activeLabel;
            }

            return pendingLabel;
        }

        private string GetStateColor(QuestManager.ObjectiveStatus status) {
            if (status.completed) {
                return completeColor;
            }

            if (status.type == QuestManager.QuestObjectiveType.ReachLocation && status.distance <= status.reachRadius) {
                return activeColor;
            }

            return pendingColor;
        }

        private string GetProgressText(QuestManager.ObjectiveStatus status) {
            if (status.type == QuestManager.QuestObjectiveType.KillEnemies) {
                string text = "(" + status.currentCount + "/" + status.requiredCount + ")";
                if (status.requireSpecificWeapon && !string.IsNullOrEmpty(status.weaponName)) {
                    text += " with " + status.weaponName;
                }
                return text;
            }

            if (status.type == QuestManager.QuestObjectiveType.ReachLocation) {
                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                builder.Append("- ");
                builder.Append(status.targetName);

                if (showDistanceForLocation && !float.IsPositiveInfinity(status.distance)) {
                    builder.Append(" (");
                    builder.Append(status.distance.ToString("0.0"));
                    builder.Append("m)");
                }

                if (showStayTimerForLocation && status.requiredStaySeconds > 0f) {
                    builder.Append(" [");
                    builder.Append(Mathf.Clamp(status.timeInRange, 0f, status.requiredStaySeconds).ToString("0.0"));
                    builder.Append("/");
                    builder.Append(status.requiredStaySeconds.ToString("0.0"));
                    builder.Append("s]");
                }

                return builder.ToString();
            }

            return string.Empty;
        }

        private string ApplyColor(string text, string color) {
            if (!useRichText || string.IsNullOrEmpty(color)) {
                return text;
            }

            return "<color=" + color + ">" + text + "</color>";
        }

        private void SetText(string value) {
            if (tmpText != null) {
                tmpText.text = value;
            }

            if (uiText != null) {
                uiText.text = value;
            }
        }
    }
}
