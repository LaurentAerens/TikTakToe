import { Link } from "react-router-dom";
import { Text, Button } from "@fluentui/react-components";

const NotFound = () => (
  <div style={{ display: "flex", flexDirection: "column", alignItems: "center", justifyContent: "center", height: "100vh", gap: 16 }}>
    <Text size={900} weight="bold">404</Text>
    <Text>Page not found.</Text>
    <Link to="/">
      <Button appearance="primary">Back home</Button>
    </Link>
  </div>
);

export default NotFound;
