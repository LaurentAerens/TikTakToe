import { useState } from "react";
import {
  TabList,
  Tab,
  Card,
  Button,
  Input,
  Badge,
  Text,
  tokens,
} from "@fluentui/react-components";
import {
  Games24Regular,
  Copy24Regular,
  Checkmark24Regular,
  Send24Regular,
  BotRegular,
  People24Regular,
} from "@fluentui/react-icons";
import EngineSelector from "@/components/EngineSelector";

interface Engine {
  id: string;
  name: string;
  description?: string;
}

interface RightPanelProps {
  engines: Engine[];
  playEngine: string;
  onPlayEngineChange: (value: string) => void;
}

interface Challenge {
  id: string;
  opponentId: string;
  status: "pending" | "accepted" | "declined";
  direction: "sent" | "received";
  timestamp: Date;
}

const MOCK_CHALLENGES: Challenge[] = [
  { id: "c1", opponentId: "player_42", status: "pending", direction: "received", timestamp: new Date() },
  { id: "c2", opponentId: "engine_master", status: "accepted", direction: "sent", timestamp: new Date(Date.now() - 300000) },
  { id: "c3", opponentId: "xo_pro", status: "declined", direction: "sent", timestamp: new Date(Date.now() - 600000) },
];

const MY_ID = "you_" + Math.random().toString(36).slice(2, 7);

const RightPanel = ({ engines, playEngine, onPlayEngineChange }: RightPanelProps) => {
  const [tab, setTab] = useState<string>("engine");
  const [opponentId, setOpponentId] = useState("");
  const [challenges, setChallenges] = useState<Challenge[]>(MOCK_CHALLENGES);
  const [copied, setCopied] = useState(false);

  const handleCopyId = () => {
    navigator.clipboard.writeText(MY_ID);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const handleSendChallenge = () => {
    if (!opponentId.trim()) return;
    setChallenges([{
      id: `c${Date.now()}`,
      opponentId: opponentId.trim(),
      status: "pending",
      direction: "sent",
      timestamp: new Date(),
    }, ...challenges]);
    setOpponentId("");
  };

  const handleRespond = (id: string, accept: boolean) => {
    setChallenges(challenges.map((c) =>
      c.id === id ? { ...c, status: accept ? "accepted" : "declined" } : c
    ));
  };

  const statusColor = (status: Challenge["status"]): "brand" | "success" | "danger" => {
    switch (status) {
      case "pending": return "brand";
      case "accepted": return "success";
      case "declined": return "danger";
    }
  };

  return (
    <div className="flex flex-col h-full">
      <TabList selectedValue={tab} onTabSelect={(_, d) => setTab(d.value as string)}>
        <Tab value="engine" icon={<BotRegular />}>Engine</Tab>
        <Tab value="player" icon={<People24Regular />}>Player</Tab>
      </TabList>

      <div className="flex-1 mt-4 overflow-y-auto">
        {tab === "engine" && (
          <EngineSelector
            label="Play Engine"
            engines={engines}
            value={playEngine}
            onChange={onPlayEngineChange}
          />
        )}

        {tab === "player" && (
          <div className="flex flex-col gap-4">
            <Card style={{ padding: 12 }}>
              <div className="flex items-center gap-2 mb-2">
                <Games24Regular style={{ color: tokens.colorBrandForeground1, width: 16, height: 16 }} />
                <Text weight="semibold" size={200}>Your Player ID</Text>
              </div>
              <div className="flex items-center gap-2">
                <code
                  style={{
                    flex: 1,
                    backgroundColor: tokens.colorNeutralBackground3,
                    borderRadius: tokens.borderRadiusMedium,
                    padding: "6px 8px",
                    fontFamily: "ui-monospace, monospace",
                    fontSize: 12,
                    userSelect: "all",
                  }}
                >
                  {MY_ID}
                </code>
                <Button
                  appearance="outline"
                  size="small"
                  onClick={handleCopyId}
                  icon={copied ? <Checkmark24Regular /> : <Copy24Regular />}
                />
              </div>
            </Card>

            <form
              onSubmit={(e) => { e.preventDefault(); handleSendChallenge(); }}
              className="flex gap-2"
            >
              <Input
                placeholder="Opponent ID..."
                value={opponentId}
                onChange={(_, d) => setOpponentId(d.value)}
                size="small"
                style={{ flex: 1 }}
              />
              <Button
                type="submit"
                appearance="primary"
                size="small"
                icon={<Send24Regular />}
                disabled={!opponentId.trim()}
              />
            </form>

            <div>
              <Text size={100} style={{ color: tokens.colorNeutralForeground3, textTransform: "uppercase", letterSpacing: 1 }}>
                Challenges
              </Text>
              <div className="flex flex-col gap-2 mt-2">
                {challenges.map((c) => (
                  <Card key={c.id} style={{ padding: 10 }}>
                    <div className="flex items-center gap-2">
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-1.5 mb-0.5">
                          <Text size={200} weight="medium" truncate>{c.opponentId}</Text>
                          <Badge appearance="outline" color={statusColor(c.status)} size="small">
                            {c.status}
                          </Badge>
                        </div>
                        <Text size={100} style={{ color: tokens.colorNeutralForeground3 }}>
                          {c.direction === "sent" ? "Sent" : "Received"} · {c.timestamp.toLocaleTimeString()}
                        </Text>
                      </div>
                      {c.status === "pending" && c.direction === "received" && (
                        <div className="flex gap-1 shrink-0">
                          <Button size="small" appearance="outline" onClick={() => handleRespond(c.id, true)}>
                            Accept
                          </Button>
                          <Button size="small" appearance="subtle" onClick={() => handleRespond(c.id, false)}>
                            Decline
                          </Button>
                        </div>
                      )}
                      {c.status === "accepted" && (
                        <Button size="small" appearance="primary">Play</Button>
                      )}
                    </div>
                  </Card>
                ))}
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default RightPanel;
