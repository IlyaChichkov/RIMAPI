import os
import re
import yaml

input_dir = r'docs\_api_macroses\controllers'
output_base_dir = r'tests\bruno_api_tests'

def capitalize(s):
    if not s:
        return s
    return s[0].upper() + s[1:]

def get_action_name(path):
    parts = [p for p in path.split('/') if p and p != 'api' and p != 'v1']
    if not parts:
        return "Root"
    
    # If the last part is a parameter like {id}, use the part before it
    last = parts[-1]
    if last.startswith('{') and last.endswith('}'):
        if len(parts) > 1:
            name = parts[-2] + capitalize(last[1:-1])
        else:
            name = last[1:-1]
    else:
        name = last
    
    # Special case for camera/change/zoom -> Zoom
    if len(parts) >= 2 and parts[-2] == 'change':
        name = parts[-1]
    
    # CamelCase it
    name = ''.join(capitalize(p) for p in name.split('_'))
    return name

def process_file(file_path):
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # The files are almost YAML, but not quite because of the markdown in them.
    # However, yaml.safe_load might still work if we are lucky, or we can use regex.
    # Let's try regex for robustness.
    
    controller_name = os.path.basename(file_path).replace('Controller.yml', '').replace('.yml', '')
    output_dir = os.path.join(output_base_dir, controller_name)
    if not os.path.exists(output_dir):
        os.makedirs(output_dir)
    
    # Find all endpoints
    # Endpoints start with /api/v1/ at the beginning of a line
    endpoints = re.findall(r'^(/api/v1/[^\s:]+):', content, re.MULTILINE)
    
    seq = 1
    for path in endpoints:
        # Find the block for this endpoint
        # It ends at the next endpoint or end of file
        start_idx = content.find(path + ":")
        end_idx = content.find('\n/api/v1/', start_idx + 1)
        if end_idx == -1:
            block = content[start_idx:]
        else:
            block = content[start_idx:end_idx]
        
        # Extract method
        method_match = re.search(r'method:\s*(\w+)', block)
        method = method_match.group(1) if method_match else 'GET'
        
        # Extract curl example for URL and body
        curl_match = re.search(r'curl:\s*\|-\s*(.*?)(?:\n\w+:|\Z)', block, re.DOTALL)
        curl_example = curl_match.group(1) if curl_match else ''
        
        # Extract body from curl --data
        body = None
        if '--data \'' in curl_example:
            body_match = re.search(r'--data \'(.*?)\'', curl_example, re.DOTALL)
            if body_match:
                body = body_match.group(1).strip()
        elif '--data "{' in curl_example:
             # Handle double quotes if any
             body_match = re.search(r'--data "(.*?)"', curl_example, re.DOTALL)
             if body_match:
                body = body_match.group(1).strip()
        
        # Extract URL from curl --url
        url = "{{baseURL}}" + path
        # If there are query params in curl, use them
        url_match = re.search(r'--url \'?http://localhost:\d+(/api/v1/[^\s\']*)', curl_example)
        if url_match:
            url_path = url_match.group(1)
            url = "{{baseURL}}" + url_path
            url = '"' + url + '"'
            
        action_name = get_action_name(path)
        
        # Create Bruno YAML
        bruno_data = {
            'info': {
                'name': action_name,
                'type': 'http',
                'seq': seq
            },
            'http': {
                'method': method,
                'url': url,
                'auth': 'inherit'
            },
            'settings': {
                'encodeUrl': True,
                'timeout': 0,
                'followRedirects': True,
                'maxRedirects': 5
            }
        }
        
        if body:
            bruno_data['body'] = {'json': body}
            
        # Write file
        # We need a custom dumper to match the format exactly if possible
        # but a simple manual write is safer for the exact format required.
        
        filename = f"{action_name}.yml"
        file_out = os.path.join(output_dir, filename)
        
        with open(file_out, 'w', encoding='utf-8') as out:
            out.write(f"info:\n")
            out.write(f"  name: {bruno_data['info']['name']}\n")
            out.write(f"  type: {bruno_data['info']['type']}\n")
            out.write(f"  seq: {bruno_data['info']['seq']}\n\n")
            out.write(f"http:\n")
            out.write(f"  method: {bruno_data['http']['method']}\n")
            out.write(f"  url: {bruno_data['http']['url']}\n")
            out.write(f"  auth: {bruno_data['http']['auth']}\n\n")
            if body:
                out.write(f"body:\n")
                out.write(f"  json: |-\n")
                # Indent body
                indented_body = '\n'.join('    ' + line for line in body.splitlines())
                out.write(f"{indented_body}\n\n")
            out.write(f"settings:\n")
            out.write(f"  encodeUrl: true\n")
            out.write(f"  timeout: 0\n")
            out.write(f"  followRedirects: true\n")
            out.write(f"  maxRedirects: 5\n")
            
        seq += 1

# Process all files
for filename in os.listdir(input_dir):
    if filename.endswith('.yml') and filename != 'General.yml': # General doesn't have endpoints
        process_file(os.path.join(input_dir, filename))
