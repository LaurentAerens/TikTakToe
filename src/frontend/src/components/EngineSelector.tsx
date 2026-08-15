import { Dropdown, Option, Field, tokens } from "@fluentui/react-components";

interface Engine {
  id: string;
  name: string;
  description?: string;
}

interface EngineSelectorProps {
  label: string;
  engines: Engine[];
  value: string;
  onChange: (value: string) => void;
}

const EngineSelector = ({ label, engines, value, onChange }: EngineSelectorProps) => {
  const selected = engines.find((e) => e.id === value);

  return (
    <Field label={label} size="small">
      <Dropdown
        value={selected?.name ?? ""}
        selectedOptions={[value]}
        onOptionSelect={(_, data) => {
          if (data.optionValue) onChange(data.optionValue);
        }}
        size="small"
      >
        {engines.map((engine) => (
          <Option key={engine.id} value={engine.id} text={engine.name}>
            <div className="flex flex-col">
              <span>{engine.name}</span>
              {engine.description && (
                <span style={{ fontSize: 11, color: tokens.colorNeutralForeground3 }}>
                  {engine.description}
                </span>
              )}
            </div>
          </Option>
        ))}
      </Dropdown>
    </Field>
  );
};

export default EngineSelector;
