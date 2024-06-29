import Catalog from "../../features/catalog/Catalog";
import Header from "./Header";
import { Container, CssBaseline } from "@mui/material";

function App() {

  return (
    <>
      <CssBaseline />
      <Header />
      <Container>
        <Catalog />
      </Container>

    </>
  )
}

export default App
