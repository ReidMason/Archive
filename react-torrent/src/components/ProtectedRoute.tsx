import React from 'react';
import { Redirect, Route } from 'react-router-dom';

interface ProtectedRouteProps {
    component: React.ComponentType<any>;
    authenticated: boolean;
    // Catch all other props
    [x: string]: any;
}

export default function ProtectedRoute({ component: Component, authenticated, ...rest }: ProtectedRouteProps) {
    return (
        <div>
            <Route {...rest} render={(props) => {
                if (authenticated)
                    return <Component {...props} />

                else {
                    return <Redirect to={{ pathname: "/login", state: { from: props.location } }} />
                }
            }} />
        </div>
    )
}
