import { Avatar, Button, Card, CardActions, CardContent, CardHeader, CardMedia, Typography } from "@mui/material";
import { Product } from "../../app/layout/models/product";

interface Props {
    product: Product;
}

export default function ProductCard({ product }: Props) {

    return (
        <Card>
            <CardHeader avatar={
                <Avatar>
                    {product.name.charAt(0).toUpperCase()}
                </Avatar>
            }
                title={product.name}
            />
            <CardMedia
                sx={{ height: 140 }}
                image={product.pictureUrl}
                title={product.name}
            />
            <CardContent>
                <Typography gutterBottom color='secondary' variant="h5">
                    {product.price}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                    {product.brand} / {product.type}
                </Typography>
            </CardContent>
            <CardActions>
                <Button size="small">Add to cart</Button>
                <Button size="small">View</Button>
            </CardActions>
        </Card>
    )
}