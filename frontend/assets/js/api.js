// This file handles API calls and data fetching for the application.

const apiBaseUrl = 'https://api.example.com'; // Replace with your API base URL

/**
 * Fetch data from the API
 * @param {string} endpoint - The API endpoint to fetch data from
 * @returns {Promise<Object>} - The data returned from the API
 */
async function fetchData(endpoint) {
    try {
        const response = await fetch(`${apiBaseUrl}/${endpoint}`);
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        return await response.json();
    } catch (error) {
        console.error('There was a problem with the fetch operation:', error);
    }
}

/**
 * Example function to get user data
 * @returns {Promise<Object>} - The user data
 */
async function getUserData() {
    return await fetchData('users');
}

/**
 * Example function to get posts
 * @returns {Promise<Object>} - The posts data
 */
async function getPosts() {
    return await fetchData('posts');
}

// Export functions for use in other modules
export { fetchData, getUserData, getPosts };